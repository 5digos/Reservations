using Application.Interfaces.IServices;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly HttpClient _httpClient;

        public VehicleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> ExistsAsync(Guid vehicleId)

        {
            //Todavia no tenesmo VehicleMS
            //var response = await _httpClient.GetAsync($"/api/vehicles/{vehicleId}");
            //return response.IsSuccessStatusCode;

            // Simulación temporal:
            await Task.Delay(10);
            return true;
        }
    }
}
