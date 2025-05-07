using Application.Dtos.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices.IVehicleServices
{
    public interface IVehicleService
    {
        Task<decimal> GetHourlyRate(Guid vehicleId);
        Task<bool> Exists(Guid vehicleId);
        Task<int> GetStatusId(Guid vehicleId);
        Task<bool> BranchOfficeExists(int branchOfficeId);
        Task<string> GetBranchOfficeName(int branchOfficeId);
        Task<List<VehicleSummaryDto>> GetVehicles(
            int branchOfficeId,
            bool onlyStatusAvailable,
            int? category = null,
            int? seatingCapacity = null,
            int? transmissionType = null,
            decimal? maxPrice = null,
            string color = null,
            string brand = null,
            int offset = 0,
            int size = 10000
        );
    }
}
