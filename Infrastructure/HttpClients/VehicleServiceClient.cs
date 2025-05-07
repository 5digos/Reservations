using Application.Interfaces.IServices.IVehicleServices;
using Infrastructure.HttpClients.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HttpClients
{
    public class VehicleServiceClient : IVehicleService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VehicleServiceClient> _logger;

        public VehicleServiceClient(HttpClient httpClient, ILogger<VehicleServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }        

        /// <summary>
        /// Comprueba si existe un vehículo con el ID dado.
        /// Usa un endpoint GET /api/v1/vehicles/{vehicleId}/exists que devuelva 200 OK si existe, 404 si no.
        /// </summary>
        public async Task<bool> Exists(Guid vehicleId)
        {
            var response = await _httpClient.GetAsync($"api/v1/vehicles/{vehicleId}/exists");
            if (response.IsSuccessStatusCode)
                return true;
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            // Log e interpretar otros códigos como error
            _logger.LogError("Error checking vehicle existence: {StatusCode}", response.StatusCode);
            response.EnsureSuccessStatusCode();
            return false; // unreachable
        }        

        /// <summary>
        /// Obtiene la tarifa por hora del vehículo.
        /// Endpoint GET /api/v1/vehicles/{vehicleId}/rate devuelve { "rate": 123.45 }
        /// </summary>
        public async Task<decimal> GetHourlyRate(Guid vehicleId)
        {
            var response = await _httpClient.GetAsync($"api/v1/vehicles/{vehicleId}/rate");
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<HourlyRateDto>();
            if (dto == null)
                throw new InvalidOperationException($"Empty response retrieving rate for vehicle {vehicleId}");

            return dto.Rate;
        }

        public async Task<int> GetStatusId(Guid vehicleId)
        {
            var response = await _httpClient.GetAsync($"api/v1/vehicles/{vehicleId}/status");
            response.EnsureSuccessStatusCode();
            var dto = await response.Content.ReadFromJsonAsync<VehicleStatusDto>();
            if (dto == null)
                throw new InvalidOperationException($"Empty status response for vehicle {vehicleId}");
            return dto.Id;
        }

        public async Task<bool> BranchOfficeExists(int branchOfficeId)
        {
            var response = await _httpClient.GetAsync($"api/v1/branchOffices/{branchOfficeId}/exists");
            if (response.IsSuccessStatusCode) return true;
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
            _logger.LogError("Error checking branch existence: {StatusCode}", response.StatusCode);
            response.EnsureSuccessStatusCode();
            return false;
        }

        public async Task<string> GetBranchOfficeName(int branchOfficeId)
        {
            var response = await _httpClient.GetAsync($"api/v1/branchOffices/{branchOfficeId}/name");
            response.EnsureSuccessStatusCode();
            var dto = await response.Content.ReadFromJsonAsync<BranchOfficeDto>();
            if (dto == null)
                throw new InvalidOperationException($"Empty branch name for {branchOfficeId}");
            return dto.Name;
        }

        public async Task<List<Application.Dtos.External.VehicleSummaryDto>> GetVehicles(
            int branchOfficeId,
            bool onlyStatusAvailable,
            int? category = null,
            int? seatingCapacity = null,
            int? transmissionType = null,
            decimal? maxPrice = null,
            string color = null,
            string brand = null,
            int offset = 0,
            int size = 10000)
        {
            var query = new List<string>
            {
                $"branchOffice={branchOfficeId}",
                onlyStatusAvailable ? "status=1" : ""
            };
            if (category.HasValue) query.Add($"category={category}");
            if (seatingCapacity.HasValue) query.Add($"seatingCapacity={seatingCapacity}");
            if (transmissionType.HasValue) query.Add($"transmissionType={transmissionType}");
            if (maxPrice.HasValue) query.Add($"maxPrice={maxPrice}");
            if (!string.IsNullOrEmpty(color)) query.Add($"color={WebUtility.UrlEncode(color)}");
            if (!string.IsNullOrEmpty(brand)) query.Add($"brand={WebUtility.UrlEncode(brand)}");
            query.Add($"offset={offset}");
            query.Add($"size={size}");

            var url = "api/v1/vehicles?" + string.Join("&", query.Where(q => !string.IsNullOrEmpty(q)));
            return await _httpClient.GetFromJsonAsync<List<Application.Dtos.External.VehicleSummaryDto>>(url);
        }
    }
    
}
