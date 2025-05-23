using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices.IReservationServices;
using Application.Interfaces.IServices.IVehicleServices;
using Domain.Enums;
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

            // recuperar nombres de sucursales
            var pickup = await _vehicleService.GetBranchOfficeByIdAsync(res.PickupBranchOfficeId);
            var dropoff = await _vehicleService.GetBranchOfficeByIdAsync(res.DropOffBranchOfficeId);

            return new ReservationResponse
            {
                ReservationId = res.ReservationId,
                UserId = res.UserId,
                VehicleId = res.VehicleId,
                PickupBranchOfficeId = res.PickupBranchOfficeId,
                PickupBranchOfficeName = pickup?.Name!,
                DropOffBranchOfficeId = res.DropOffBranchOfficeId,
                DropOffBranchOfficeName = dropoff?.Name!,
                StartTime = res.StartTime,
                EndTime = res.EndTime,
                ActualPickupTime = res.ActualPickupTime,
                ActualReturnTime = res.ActualReturnTime,
                HourlyRateSnapshot = res.HourlyRateSnapshot,
                OriginalCost = res.OriginalCost,
                LateFee = res.LateFee,
                Status = res.Status
            };
        }

        public async Task<PagedResult<ReservationSummaryResponse>> GetReservationsAsync(
            int userId,
            ReservationStatus? status,
            DateTime? from,
            DateTime? to,
            int? offset,
            int? size)
        {
            var (reservas, total) = await _reservationQuery.GetReservationsAsync(
                userId, status, from, to, offset, size);

            var dtos = new List<ReservationSummaryResponse>();
            foreach (var r in reservas)
            {
                // recuperar nombres de sucursales
                var pickup = await _vehicleService.GetBranchOfficeByIdAsync(r.PickupBranchOfficeId);
                var dropoff = await _vehicleService.GetBranchOfficeByIdAsync(r.DropOffBranchOfficeId);

                dtos.Add(new ReservationSummaryResponse
                {
                    ReservationId = r.ReservationId,                    
                    VehicleId = r.VehicleId,                    
                    PickupBranchOfficeName = pickup?.Name!,                    
                    DropOffBranchOfficeName = dropoff?.Name!,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,                    
                    Status = r.Status                    
                });
            }

            return new PagedResult<ReservationSummaryResponse>
            {
                Items = dtos,
                TotalCount = total
            };
        }
    }
    
}
