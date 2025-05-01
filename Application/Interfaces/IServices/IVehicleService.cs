using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.Request;

namespace Application.Interfaces.IServices
{
    public interface IVehicleService
    {
        Task<VehicleDto?> GetVehiculoByIdAsync(Guid id);
    }
}
