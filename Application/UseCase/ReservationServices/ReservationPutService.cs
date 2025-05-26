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
using System.Threading.Tasks;

namespace Application.UseCase.ReservationServices
{
    public class ReservationPutService : IReservationPutService
    {
        private readonly IReservationCommand _reservationCommand;
        private readonly IReservationEventCommand _eventCommand;
        private readonly IVehicleService _vehicleService;
        private readonly IReservationQuery _reservationQuery;
        private readonly ITimeProvider _timeProvider;
        private readonly INotificationService _notificationService;


        public ReservationPutService(
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

        public async Task<ReservationResponse> Update(int userId, Guid reservationId, ReservationUpdateRequest req)
        {            
            var res = await _reservationQuery.GetById(reservationId)
                      ?? throw new NotFoundException("Reserva no encontrada.");
            
            if (res.UserId != userId)
                throw new UnauthorizedAccessException("No tienes permiso para modificar esta reserva.");

            
            var pickup = await _vehicleService.GetBranchOfficeByIdAsync(req.PickupBranchOfficeId)
                          ?? throw new NotFoundException("Sucursal de recogida no encontrada.");

            var dropoff = await _vehicleService.GetBranchOfficeByIdAsync(req.DropOffBranchOfficeId)
                          ?? throw new NotFoundException("Sucursal de devolución no encontrada.");

            var nextPickup = await _reservationQuery.GetNextPickupBranchExceptAsync(
            vehicleId: res.VehicleId,
            afterTime: req.EndTime,
            excludeReservationId: reservationId
            );

            if (nextPickup.HasValue && nextPickup.Value != req.DropOffBranchOfficeId)
                throw new InvalidValueException(
                    "Este vehículo tiene una reserva próxima en otra sucursal; no puede devolverse en una diferente."
                );

            var lastDropOff = await _reservationQuery.GetLastReturnBranchExceptAsync(
            vehicleId: res.VehicleId,
            beforeTime: req.StartTime,
            excludeReservationId: reservationId
            );

            if (lastDropOff.HasValue && lastDropOff.Value != req.PickupBranchOfficeId)
                throw new InvalidValueException(
                    "El vehículo no estará disponible en la sucursal de recogida al inicio de la reserva."
                );

            if (await _reservationQuery.HasOverlapExceptAsync(
                    res.VehicleId,
                    req.StartTime,
                    req.EndTime,
                    bufferHours: 3,
                    excludeReservationId: reservationId))
            {
                throw new InvalidValueException("Los nuevos horarios chocan con otra reserva.");
            }

            
            res.PickupBranchOfficeId = req.PickupBranchOfficeId;
            res.DropOffBranchOfficeId = req.DropOffBranchOfficeId;
            res.StartTime = req.StartTime;
            res.EndTime = req.EndTime;

            await _reservationCommand.Update(res);

            
            var now = _timeProvider.Now;
            await _eventCommand.Add(new ReservationEvent
            {
                EventId = Guid.NewGuid(),
                ReservationId = reservationId,
                EventType = ReservationEventType.Updated,
                OccurredAt = now,
                Details = "Reserva actualizada por el usuario"
            });

            // Encolar notificación
            await _notificationService.EnqueueEvent(new NotificationEventRequest
            {
                UserId = userId,
                EventType = "ReservationUpdated",
                Payload = new
                {
                    ReservationId = res.ReservationId,
                    PickupBranchName = pickup.Name,
                    DropOffBranchName = dropoff.Name,
                    StartTime = req.StartTime,
                    EndTime = req.EndTime
                }
            });

            return new ReservationResponse
            {
                ReservationId = res.ReservationId,
                UserId = res.UserId,
                VehicleId = res.VehicleId,
                PickupBranchOfficeId = res.PickupBranchOfficeId,
                PickupBranchOfficeName = pickup.Name,
                DropOffBranchOfficeId = res.DropOffBranchOfficeId,
                DropOffBranchOfficeName = dropoff.Name,
                StartTime = res.StartTime,
                EndTime = res.EndTime,
                Status = res.Status
            };
        }
    }
}
