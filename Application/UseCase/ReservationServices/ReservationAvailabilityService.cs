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
            int? offset,
            int? size,
            int? category = null,
            int? seatingCapacity = null,
            int? transmissionType = null,
            decimal? maxPrice = null,
            string? color = null,
            string? brand = null)
        {
            // 1) Traer todos los vehículos “estáticos” del branchOffice según filtros
            var dtos = await _vehicleService.GetVehiclesAsync(
                branchOfficeId,
                startTime,
                endTime,
                category,
                seatingCapacity,
                transmissionType,
                maxPrice,
                color,
                brand,
                offset: 0,
                size: int.MaxValue
            );

            var available = new List<VehicleSummaryResponse>();

            foreach (var dto in dtos)
            {
                // 2) Calcular dónde está el vehículo al startTime
                var lastReturnBranch = await _reservationQuery
                    .GetLastReturnBranch(dto.Id, startTime);

                var locationAtStart = lastReturnBranch ?? dto.BranchOfficeId;
                if (locationAtStart != branchOfficeId)
                    continue;

                // 3) Verificar solapamiento con buffer 
                bool hasOverlap = await _reservationQuery
                    .HasOverlap(
                        vehicleId: dto.Id,
                        start: startTime,
                        end: endTime,
                        bufferHours: 3
                    );

                if (hasOverlap)
                    continue;

                // 4) Mapeo a tu DTO de respuesta
                available.Add(new VehicleSummaryResponse
                {
                    Id = dto.Id,
                    Brand = dto.Brand,
                    Model = dto.Model,
                    Price = dto.Price,
                    SeatingCapacity = dto.SeatingCapacity,
                    TransmissionType = dto.TransmissionType,
                    Category = dto.Category,                    
                    ImageUrl = dto.ImageUrl
                });
            }

            // 5) Paginación in-memory
            var total = available.Count;
            var page = available
                .Skip(offset.Value)
                .Take(size.Value)
                .ToList();

            return new PagedResult<VehicleSummaryResponse>
            {
                Items = page,
                TotalCount = total
            };
        }
    }
}
