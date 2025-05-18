using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices.IReservationServices;
using Application.Interfaces.IServices.IVehicleServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase.ReservationServices
{
    public class ReservationGetService : IReservationGetService
    {
        private readonly IReservationQuery _reservationQuery;
        private readonly IVehicleService _vehicleService;

        public ReservationGetService(
            IReservationQuery reservationQuery,
            IVehicleService vehicleService)
        {
            _reservationQuery = reservationQuery;
            _vehicleService = vehicleService;
        }


        public async Task<ReservationResponse?> GetById(Guid reservationId)
        {
            var res = await _reservationQuery.GetById(reservationId);
            if (res == null)
                throw new NotFoundException("Reserva no encontrada.");

            // Obtener nombres de sucursales
            var pickupBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.PickupBranchOfficeId);
            var dropOffBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.DropOffBranchOfficeId);

            return new ReservationResponse
            {
                ReservationId = res.ReservationId,
                UserId = res.UserId,
                VehicleId = res.VehicleId,
                PickupBranchOfficeId = res.PickupBranchOfficeId,
                PickupBranchOfficeName = pickupBranch?.Name,
                DropOffBranchOfficeId = res.DropOffBranchOfficeId,
                DropOffBranchOfficeName = dropOffBranch?.Name,
                StartTime = res.StartTime,
                EndTime = res.EndTime,
                ActualPickupTime = res.ActualPickupTime,
                ActualReturnTime = res.ActualReturnTime,
                HourlyRateSnapshot = res.HourlyRateSnapshot,
                OriginalCost = res.OriginalCost,
                LateFee = res.LateFee,
                Status = res.Status,
                CreatedAt = res.CreatedAt
            };
        }
    }
    
}
