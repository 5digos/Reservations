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
    public class ReservationAvailabilityService : IReservationAvailabilityService
    {
        private readonly IVehicleService _vehicleService;
        private readonly IReservationQuery _reservationQuery;

        public ReservationAvailabilityService(
            IVehicleService vehicleService,
            IReservationQuery reservationQuery)
        {
            _vehicleService = vehicleService;
            _reservationQuery = reservationQuery;
        }

        public async Task<PagedResult<VehicleSummaryResponse>> GetAvailableVehiclesAsync(
            int branchOfficeId,
            DateTime startTime,
            DateTime endTime,
            int offset,
            int size,
            int? category = null,
            int? seatingCapacity = null,
            int? transmissionType = null,
            decimal? maxPrice = null,
            string color = null,
            string brand = null)
        {
            
            var dtos = await _vehicleService.GetVehicles(
                branchOfficeId,
                onlyStatusAvailable: true,
                category, seatingCapacity, transmissionType,
                maxPrice, color, brand,
                offset: 0,     
                size: 10000);  

            var filtered = new List<VehicleSummaryResponse>();

            foreach (var v in dtos)
            {
                // 2) Ubicación futura
                var lastReturn = await _reservationQuery
                    .GetLastReturnBranch(v.Id, startTime);
                var locationAtStart = lastReturn ?? v.BranchOfficeId;
                if (locationAtStart != branchOfficeId) continue;

                // 3) Solapamiento de reservas
                bool hasOverlap = await _reservationQuery
                       .HasOverlap(
                           vehicleId: v.Id,
                           start: startTime,
                           end: endTime,
                           bufferHours: 2);

                if (hasOverlap)
                    continue;

                // 4) Mapear a DTO para el front
                filtered.Add(new VehicleSummaryResponse
                {
                    Id = v.Id,
                    Brand = v.Brand,
                    Model = v.Model,
                    HourlyRate = v.Price,
                    ImageUrl = v.ImageUrl,
                    CategoryName = v.Category.Name,
                    SeatingCapacity = v.SeatingCapacity
                });
            }

            // 5) Paginación in-memory
            var total = filtered.Count;
            var page = filtered
                .Skip(offset)
                .Take(size)
                .ToList();

            return new PagedResult<VehicleSummaryResponse>
            {
                Items = page,
                TotalCount = total
            };
        }
    }
}
