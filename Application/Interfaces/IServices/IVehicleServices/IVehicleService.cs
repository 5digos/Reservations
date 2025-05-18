using Application.Dtos.External;
using Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices.IVehicleServices
{
    public interface IVehicleService
    {
        //Task<VehicleDetailResponse?> GetVehicleByIdAsync(Guid vehicleId);
        //Task<BranchOfficeDetailResponse?> GetBranchOfficeByIdAsync(int branchOfficeId);
        //Task<List<VehicleSummaryResponse>> GetVehiclesAsync(
        //    int branchOfficeId,
        //    bool onlyStatusAvailable,
        //    int? category = null,
        //    int? seatingCapacity = null,
        //    int? transmissionType = null,
        //    decimal? maxPrice = null,
        //    string? color = null,
        //    string? brand = null,
        //    int offset = 0,
        //    int size = 10000
        //);

        Task<List<VehicleSummaryDto>> GetVehiclesAsync(
            int branchOfficeId,
            DateTime startTime,
            DateTime endTime,
            int? category = null,
            int? seatingCapacity = null,
            int? transmissionType = null,
            decimal? maxPrice = null,
            string? color = null,
            string? brand = null,
            int? offset = null,
            int? size = null);

        Task<VehicleDetailDto> GetVehicleByIdAsync(Guid vehicleId);

        Task<BranchOfficeDto> GetBranchOfficeByIdAsync(int branchOfficeId);
    }
}
