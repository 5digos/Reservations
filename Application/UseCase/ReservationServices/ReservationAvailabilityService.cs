using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
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
        private readonly ITimeProvider _clock;

        public ReservationAvailabilityService(
            IVehicleService vehicleService,
            IReservationQuery reservationQuery,
            ITimeProvider clock)

        {
            _vehicleService = vehicleService;
            _reservationQuery = reservationQuery;
            _clock = clock;
        }

        public async Task<PagedResult<VehicleSummaryResponse>> GetAvailableVehiclesAsync(
            int pickupBranchOfficeId,
            int dropOffBranchOfficeId,
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

            // Regla: solo reservas para hoy o mañana ───────────────────────────
            if (startTime.Date > _clock.Now.Date.AddDays(1))
                throw new InvalidValueException("Solo puedes reservar para el día de hoy o mañana.");

            // Traer todos los vehículos “estáticos” del branchOffice según filtros
            var candidates = await _vehicleService.GetVehiclesAsync(
                null,
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

            foreach (var v in candidates)
            {
                // (a) Vehículo debe estar físicamente en la sucursal de retiro
                var branchAtStart = await _reservationQuery.GetLastReturnBranch(v.Id, startTime)
                                   ?? v.BranchOfficeId;

                if (branchAtStart != pickupBranchOfficeId) continue;

                // (b) No debe haber solapamiento + buffer
                if (await _reservationQuery.HasOverlap(v.Id, startTime, endTime, bufferHours: 3))
                    continue;

                // (c) Si existe una reserva futura, su pickup Sucursal debe coincidir
                //     con la sucursal de devolución que pide el usuario
                var nextPickup = await _reservationQuery.GetNextPickupBranch(v.Id, endTime);
                if (nextPickup.HasValue && nextPickup.Value != dropOffBranchOfficeId)
                    continue;

                // Mapeo a tu DTO de respuesta
                available.Add(new VehicleSummaryResponse
                {
                    Id = v.Id,
                    Brand = v.Brand,
                    Model = v.Model,
                    Price = v.Price,
                    SeatingCapacity = v.SeatingCapacity,
                    TransmissionType = v.TransmissionType,
                    Category = v.Category,
                    Color = v.Color,
                    ImageUrl = v.ImageUrl
                });
            }
            

            // Paginación in-memory
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
