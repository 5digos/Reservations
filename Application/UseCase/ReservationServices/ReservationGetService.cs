using Application.Dtos.Response;
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


        public async Task<ReservationResponse> GetById(Guid reservationId)
        {
            var res = await _reservationQuery.GetById(reservationId);
            if (res == null) return null;

            // Obtener nombres de sucursales
            var pickupName = await _vehicleService.GetBranchOfficeName(res.PickupBranchOfficeId);
            var dropoffName = await _vehicleService.GetBranchOfficeName(res.DropOffBranchOfficeId);

            return new ReservationResponse
            {
                ReservationId = res.ReservationId,
                UserId = res.UserId,
                VehicleId = res.VehicleId,
                PickupBranchOfficeId = res.PickupBranchOfficeId,
                PickupBranchOfficeName = pickupName,
                DropOffBranchOfficeId = res.DropOffBranchOfficeId,
                DropOffBranchOfficeName = dropoffName,
                StartTime = res.StartTime,
                EndTime = res.EndTime,
                HourlyRateSnapshot = res.HourlyRateSnapshot
            };
        }
    }
    
}
