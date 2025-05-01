using System;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices
{
    public interface IVehicleService
    {
        Task<bool> ExistsAsync(Guid vehicleId);
    }
}
