using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
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
    public class ReservationPostService : IReservationPostService
    {
        private readonly IReservationQuery _reservationQuery;
        private readonly IReservationCommand _reservationCommand;
        private readonly IReservationEventCommand _eventCommand;
        private readonly IVehicleService _vehicleService;

        public ReservationPostService(
            IReservationQuery reservationQuery,
            IReservationCommand reservationCommand,
            IReservationEventCommand eventCommand,
            IVehicleService vehicleService)
        {
            _reservationQuery = reservationQuery;
            _reservationCommand = reservationCommand;
            _eventCommand = eventCommand;
            _vehicleService = vehicleService;
        }

        public async Task<ReservationResponse> Create(int userId, ReservationRequest request)
        {
            //Validar vehículo existe
            if (!await _vehicleService.Exists(request.VehicleId))
                throw new InvalidOperationException("El vehículo no existe.");

            //Validar estado “Disponible” (VehicleStatusId = 1)
            var statusId = await _vehicleService.GetStatusId(request.VehicleId);
            if (statusId != 1)
                throw new InvalidOperationException(
                    "El vehículo no está en estado 'Disponible' y no puede reservarse.");

            //Validar sucursales existen
            if (!await _vehicleService.BranchOfficeExists(request.PickupBranchOfficeId))
                throw new InvalidOperationException("La sucursal de recogida no existe.");
            if (!await _vehicleService.BranchOfficeExists(request.DropOffBranchOfficeId))
                throw new InvalidOperationException("La sucursal de devolución no existe.");

            //Validar solapamiento de reservas
            bool overlap = await _reservationQuery.HasOverlap(
                request.VehicleId, request.StartTime, request.EndTime);
            if (overlap)
                throw new InvalidOperationException("Vehículo no disponible en el rango solicitado.");

            //Obtener tarifa del vehículo
            decimal rate = await _vehicleService.GetHourlyRate(request.VehicleId);


            //Crear entidad reserva
            var reservation = new Reservation
            {
                ReservationId = Guid.NewGuid(),
                UserId = userId,
                VehicleId = request.VehicleId,
                PickupBranchOfficeId = request.PickupBranchOfficeId,
                DropOffBranchOfficeId = request.DropOffBranchOfficeId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                HourlyRateSnapshot = rate,
                Status = ReservationStatus.Confirmed
            };

            //Guardar reserva
            await _reservationCommand.Add(reservation);

            //Registrar evento Created
            var evt = new ReservationEvent
            {
                EventId = Guid.NewGuid(),
                ReservationId = reservation.ReservationId,
                EventType = ReservationEventType.Created,
                OccurredAt = DateTime.Now,
                Details = "Reserva creada"
            };
            await _eventCommand.Add(evt);

            //Obtener nombres de sucursales para el DTO
            var pickupName = await _vehicleService.GetBranchOfficeName(reservation.PickupBranchOfficeId);
            var dropoffName = await _vehicleService.GetBranchOfficeName(reservation.DropOffBranchOfficeId);

            //Retornar respuesta
            return new ReservationResponse
            {
                ReservationId = reservation.ReservationId,
                UserId = reservation.UserId,
                VehicleId = reservation.VehicleId,
                PickupBranchOfficeId = reservation.PickupBranchOfficeId,
                PickupBranchOfficeName = pickupName,
                DropOffBranchOfficeId = reservation.DropOffBranchOfficeId,
                DropOffBranchOfficeName = dropoffName,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                HourlyRateSnapshot = rate
            };
        }
    }
}
