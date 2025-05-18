using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IReservationServices;
using Application.Interfaces.IServices.IVehicleServices;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.UseCase.ReservationServices
{
    public class ReservationPostService : IReservationPostService
    {
        private readonly IReservationCommand _reservationCommand;
        private readonly IReservationEventCommand _eventCommand;
        private readonly IVehicleService _vehicleService;
        private readonly IReservationQuery _reservationQuery;
        private readonly ITimeProvider _timeProvider;
        private readonly INotificationService _notificationService;

        public ReservationPostService(
            IReservationCommand reservationCommand,
            IReservationEventCommand eventCommand,
            IVehicleService vehicleService,
            IReservationQuery reservationQuery,
            ITimeProvider timeProvider,
            INotificationService notificationService)
        {
            _reservationCommand = reservationCommand;
            _eventCommand = eventCommand;
            _vehicleService = vehicleService;
            _reservationQuery = reservationQuery;
            _timeProvider = timeProvider;
            _notificationService = notificationService;
        }

        public async Task<ReservationResponse> Create(int userId, ReservationRequest request)
        {
            // Validar sucursales
            var pickupBranch = await _vehicleService.GetBranchOfficeByIdAsync(request.PickupBranchOfficeId)
                ?? throw new NotFoundException("Sucursal de recogida no encontrada.");
            var dropOffBranch = await _vehicleService.GetBranchOfficeByIdAsync(request.DropOffBranchOfficeId)
                ?? throw new NotFoundException("Sucursal de devolución no encontrada.");

            // Validar vehículo
            var vehicle = await _vehicleService.GetVehicleByIdAsync(request.VehicleId)
                ?? throw new NotFoundException("Vehículo no encontrado.");
            if (vehicle.StatusId != 1)
                throw new InvalidValueException("Vehículo no disponible.");

            var rateSnapshot = vehicle.Price;

            // Validar solapamiento
            if (await _reservationQuery.HasOverlap(
                request.VehicleId,
                request.StartTime,
                request.EndTime,
                bufferHours: 3))
            {
                throw new InvalidValueException("El vehículo no está disponible en el intervalo seleccionado.");
            }

            // Crear reserva
            var now = _timeProvider.Now;
            var reservation = new Reservation
            {
                ReservationId = Guid.NewGuid(),
                UserId = userId,
                VehicleId = request.VehicleId,
                PickupBranchOfficeId = request.PickupBranchOfficeId,
                DropOffBranchOfficeId = request.DropOffBranchOfficeId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = ReservationStatus.Pending,
                CreatedAt = now,
                HourlyRateSnapshot = rateSnapshot
            };
            await _reservationCommand.Add(reservation);

            // Registrar evento
            var evt = new ReservationEvent
            {
                EventId = Guid.NewGuid(),
                ReservationId = reservation.ReservationId,
                EventType = ReservationEventType.Created,
                OccurredAt = now
            };
            await _eventCommand.Add(evt);

            // Encolar notificación
            await _notificationService.EnqueueEvent(new NotificationEventRequest
            {
                UserId = userId,
                EventType = "ReservationCreated",
                Payload = JsonSerializer.Serialize(new
                {
                    reservation.ReservationId,
                    pickupBranch.Name,
                    request.StartTime
                })
            });

            // Mapear respuesta
            return new ReservationResponse
            {
                ReservationId = reservation.ReservationId,
                UserId = userId,
                VehicleId = request.VehicleId,
                PickupBranchOfficeId = pickupBranch.BranchOfficeId,
                PickupBranchOfficeName = pickupBranch.Name,
                DropOffBranchOfficeId = dropOffBranch.BranchOfficeId,
                DropOffBranchOfficeName = dropOffBranch.Name,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = reservation.Status,
                CreatedAt = reservation.CreatedAt
            };
        }
    }
}
