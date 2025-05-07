using Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices.IReservationServices
{
    public interface IReservationAvailabilityService
    {
        Task<PagedResult<VehicleSummaryResponse>> GetAvailableVehiclesAsync(
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
        string brand = null);
    }
}
