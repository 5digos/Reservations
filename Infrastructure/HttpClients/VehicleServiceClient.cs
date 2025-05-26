using Application.Dtos.External;
using Application.Dtos.Response;
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
    //public class VehicleServiceClient : IVehicleService
    //{
    //    private readonly HttpClient _httpClient;
    //    private readonly ILogger<VehicleServiceClient> _logger;

    //    public VehicleServiceClient(HttpClient httpClient, ILogger<VehicleServiceClient> logger)
    //    {
    //        _httpClient = httpClient;
    //        _logger = logger;
    //    }

    //    /// <inheritdoc />
    //    public async Task<VehicleDetailResponse> GetVehicleByIdAsync(Guid vehicleId)
    //    {
    //        var response = await _httpClient.GetAsync($"api/v1/vehicle/{vehicleId}");
    //        if (response.StatusCode == HttpStatusCode.NotFound)
    //            return null;

    //        response.EnsureSuccessStatusCode();
    //        var infra = await response.Content.ReadFromJsonAsync<VehicleDetailInfraDto>();
    //        if (infra == null)
    //            throw new InvalidOperationException($"Empty response for vehicle {vehicleId}");

    //        return new VehicleDetailResponse
    //        {
    //            Id = infra.Id,
    //            BranchOfficeId = infra.BranchOfficeId,
    //            StatusId = infra.StatusId,
    //            Price = infra.Price,
    //            CategoryId = infra.Category.Id,
    //            TransmissionTypeId = infra.TransmissionType.Id,
    //            SeatingCapacity = infra.SeatingCapacity
    //        };
    //    }

    //    /// <inheritdoc />
    //    public async Task<BranchOfficeDetailResponse> GetBranchOfficeByIdAsync(int branchOfficeId)
    //    {
    //        var response = await _httpClient.GetAsync($"api/v1/branchOffice/{branchOfficeId}");
    //        if (response.StatusCode == HttpStatusCode.NotFound)
    //            return null;

    //        response.EnsureSuccessStatusCode();
    //        var infra = await response.Content.ReadFromJsonAsync<BranchOfficeInfraDto>();
    //        if (infra == null)
    //            throw new InvalidOperationException($"Empty response for branch office {branchOfficeId}");

    //        return new BranchOfficeDetailResponse
    //        {
    //            Id = infra.Id,
    //            Name = infra.Name,
    //            City = infra.City,
    //            PostalCode = infra.PostalCode
    //        };
    //    }

    //    /// <inheritdoc />
    //    public async Task<List<VehicleSummaryResponse>> GetVehiclesAsync(
    //        int branchOfficeId,
    //        bool onlyStatusAvailable,
    //        int? category = null,
    //        int? seatingCapacity = null,
    //        int? transmissionType = null,
    //        decimal? maxPrice = null,
    //        string? color = null,
    //        string? brand = null,
    //        int offset = 0,
    //        int size = 10000)
    //    {
    //        // Construir parámetros de query
    //        var queryParams = new List<string>
    //        {
    //            $"branchOffice={branchOfficeId}",
    //            onlyStatusAvailable ? "status=1" : string.Empty
    //        };
    //        if (category.HasValue) queryParams.Add($"category={category}");
    //        if (seatingCapacity.HasValue) queryParams.Add($"seatingCapacity={seatingCapacity}");
    //        if (transmissionType.HasValue) queryParams.Add($"transmissionType={transmissionType}");
    //        if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice}");
    //        if (!string.IsNullOrEmpty(color)) queryParams.Add($"color={WebUtility.UrlEncode(color)}");
    //        if (!string.IsNullOrEmpty(brand)) queryParams.Add($"brand={WebUtility.UrlEncode(brand)}");
    //        queryParams.Add($"offset={offset}");
    //        queryParams.Add($"size={size}");

    //        var url = "api/v1/vehicle?" + string.Join("&", queryParams.Where(p => !string.IsNullOrEmpty(p)));
    //        var infraList = await _httpClient.GetFromJsonAsync<List<VehicleSummaryInfraDto>>(url);
    //        if (infraList == null)
    //            return new List<VehicleSummaryResponse>();

    //        // Mapear infra DTOs a Application DTOs
    //        return infraList.Select(infra => new VehicleSummaryResponse
    //        {
    //            Id = infra.Id,
    //            Brand = infra.Brand,
    //            Model = infra.Model,
    //            Price = infra.Price,
    //            ImageUrl = infra.ImageUrl,
    //            SeatingCapacity = infra.SeatingCapacity,
    //            Category = new VehicleCategoryResponse
    //            {
    //                Id = infra.Category.Id,
    //                Name = infra.Category.Name
    //            },
    //            BranchOfficeId = infra.BranchOfficeId
    //        }).ToList();
    //    }
    //}


    //public class VehicleServiceClient : IVehicleService
    //{
    //    private readonly HttpClient _httpClient;
    //    private readonly ILogger<VehicleServiceClient> _logger;

    //    public VehicleServiceClient(HttpClient httpClient, ILogger<VehicleServiceClient> logger)
    //    {
    //        _httpClient = httpClient;
    //        _logger = logger;
    //    }

    //    /// <inheritdoc />
    //    public async Task<VehicleDetailResponse?> GetVehicleByIdAsync(Guid vehicleId)
    //    {
    //        var response = await _httpClient.GetAsync($"api/v1/vehicles/{vehicleId}");
    //        if (response.StatusCode == HttpStatusCode.NotFound)
    //            return null;

    //        response.EnsureSuccessStatusCode();
    //        var infra = await response.Content.ReadFromJsonAsync<VehicleDetailInfraDto>();
    //        if (infra == null)
    //            throw new InvalidOperationException($"Empty response for vehicle {vehicleId}");

    //        // Validar que vengan los objetos anidados
    //        if (infra.Category == null)
    //            throw new InvalidOperationException($"El vehículo {vehicleId} no devuelve el campo Category en la respuesta.");
    //        if (infra.TransmissionType == null)
    //            throw new InvalidOperationException($"El vehículo {vehicleId} no devuelve el campo TransmissionType en la respuesta.");

    //        return new VehicleDetailResponse
    //        {
    //            Id = infra.Id,
    //            BranchOfficeId = infra.BranchOfficeId,
    //            StatusId = infra.StatusId,
    //            Price = infra.Price,
    //            CategoryId = infra.Category.Id,
    //            TransmissionTypeId = infra.TransmissionType.Id,
    //            SeatingCapacity = infra.SeatingCapacity
    //        };
    //    }

    //    /// <inheritdoc />
    //    public async Task<BranchOfficeDetailResponse?> GetBranchOfficeByIdAsync(int branchOfficeId)
    //    {
    //        var response = await _httpClient.GetAsync($"api/v1/branchOffices/{branchOfficeId}");
    //        if (response.StatusCode == HttpStatusCode.NotFound)
    //            return null;

    //        response.EnsureSuccessStatusCode();
    //        var infra = await response.Content.ReadFromJsonAsync<BranchOfficeInfraDto>();
    //        if (infra == null)
    //            throw new InvalidOperationException($"Empty response for branch office {branchOfficeId}");

    //        return new BranchOfficeDetailResponse
    //        {
    //            Id = infra.Id,
    //            Name = infra.Name,
    //            City = infra.City,
    //            PostalCode = infra.PostalCode
    //        };
    //    }

    //    /// <inheritdoc />
    //    public async Task<List<VehicleSummaryResponse>> GetVehiclesAsync(
    //        int branchOfficeId,
    //        bool onlyStatusAvailable,
    //        int? category = null,
    //        int? seatingCapacity = null,
    //        int? transmissionType = null,
    //        decimal? maxPrice = null,
    //        string? color = null,
    //        string? brand = null,
    //        int offset = 0,
    //        int size = 10000)
    //    {
    //        var queryParams = new List<string>
    //        {
    //            $"branchOffice={branchOfficeId}",
    //            onlyStatusAvailable ? "status=1" : string.Empty
    //        };
    //        if (category.HasValue) queryParams.Add($"category={category}");
    //        if (seatingCapacity.HasValue) queryParams.Add($"seatingCapacity={seatingCapacity}");
    //        if (transmissionType.HasValue) queryParams.Add($"transmissionType={transmissionType}");
    //        if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice}");
    //        if (!string.IsNullOrEmpty(color)) queryParams.Add($"color={WebUtility.UrlEncode(color)}");
    //        if (!string.IsNullOrEmpty(brand)) queryParams.Add($"brand={WebUtility.UrlEncode(brand)}");
    //        queryParams.Add($"offset={offset}");
    //        queryParams.Add($"size={size}");

    //        var url = "api/v1/vehicles?" + string.Join("&", queryParams.Where(p => !string.IsNullOrEmpty(p)));
    //        var infraList = await _httpClient.GetFromJsonAsync<List<VehicleSummaryInfraDto>>(url);
    //        if (infraList == null)
    //            return new List<VehicleSummaryDto>();

    //        return infraList.Select(infra => new VehicleSummaryResponse
    //        {
    //            Id = infra.Id,
    //            Brand = infra.Brand,
    //            Model = infra.Model,
    //            Price = infra.Price,
    //            ImageUrl = infra.ImageUrl,
    //            SeatingCapacity = infra.SeatingCapacity,
    //            Category = new VehicleCategoryDto { Id = infra.Category!.Id, Name = infra.Category.Name },
    //            BranchOfficeId = infra.BranchOfficeId
    //        }).ToList();
    //    }
    //}

    public class VehicleServiceClient : IVehicleService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VehicleServiceClient> _logger;

        public VehicleServiceClient(HttpClient httpClient, ILogger<VehicleServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<VehicleSummaryDto>> GetVehiclesAsync(
            int? branchOfficeId,
            DateTime startTime,
            DateTime endTime,
            int? category = null,
            int? seatingCapacity = null,
            int? transmissionType = null,
            decimal? maxPrice = null,
            string? color = null,
            string? brand = null,
            int? offset = null,
            int? size = null)
        {
            // Montamos los parámetros de query (igual que el endpoint de VehicleMS)
            var query = new List<string> { $"branchOffice={branchOfficeId}" };

            if (branchOfficeId.HasValue)
                query.Add($"category={branchOfficeId.Value}");

            if (category.HasValue)
                query.Add($"category={category.Value}");

            if (seatingCapacity.HasValue)
                query.Add($"seatingCapacity={seatingCapacity.Value}");

            if (transmissionType.HasValue)
                query.Add($"transmissionType={transmissionType.Value}");

            if (maxPrice.HasValue)
                query.Add($"maxPrice={maxPrice.Value}");

            if (!string.IsNullOrEmpty(color))
                query.Add($"color={Uri.EscapeDataString(color)}");

            if (!string.IsNullOrEmpty(brand))
                query.Add($"brand={Uri.EscapeDataString(brand)}");

            if (offset.HasValue)
                query.Add($"offset={offset.Value}");

            if (size.HasValue)
                query.Add($"size={size.Value}");

            var url = "api/v1/Vehicle?" + string.Join("&", query);
            var list = await _httpClient.GetFromJsonAsync<List<VehicleResponseInfraDto>>(url);
            if (list == null)
                return new List<VehicleSummaryDto>();

            // Mapear a los DTOs de Application
            return list.Select(v => new VehicleSummaryDto
            {
                Id = v.Id,
                Brand = v.Brand,
                Model = v.Model,
                Price = v.Price,
                SeatingCapacity = v.SeatingCapacity,
                BranchOfficeId = v.BranchOffice.BranchOfficeId,
                BranchOfficeName = v.BranchOffice.Name,
                ImageUrl = v.ImageUrl,
                Category = v.Category.Name,
                Color = v.Color,
                TransmissionType = v.TransmissionType.Name
            }).ToList();
        }


        public async Task<VehicleDetailDto> GetVehicleByIdAsync(Guid vehicleId)
        {
            // Llamamos a GET /api/v1/Vehicle/{id}
            var infra = await _httpClient.GetFromJsonAsync<VehicleDetailsInfraDto>($"api/v1/Vehicle/{vehicleId}");
            if (infra == null || infra.Vehicle == null)
                throw new InvalidOperationException($"El vehículo {vehicleId} no existe o la respuesta es inválida.");

            var vehicle = infra.Vehicle;

            // Mapear sucursal
            var branch = new BranchOfficeDto
            {
                BranchOfficeId = vehicle.BranchOffice.BranchOfficeId,
                Name = vehicle.BranchOffice.Name
                // aquí podrías mapear City, PostalCode, Province si los necesitas
            };

            // Construir el DTO de detalle
            return new VehicleDetailDto
            {
                Id = vehicle.Id,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Price = vehicle.Price,
                SeatingCapacity = vehicle.SeatingCapacity,
                Color = vehicle.Color,
                ImageUrl = vehicle.ImageUrl,
                CategoryName = vehicle.Category.Name,
                TransmissionTypeName = vehicle.TransmissionType.Name,
                StatusId = vehicle.Status.Id,
                BranchOffice = branch
            };
        }


        public async Task<BranchOfficeDto> GetBranchOfficeByIdAsync(int branchOfficeId)
        {
            // Llamamos a GET /api/v1/BranchOffice/{id}
            var infra = await _httpClient.GetFromJsonAsync<BranchOfficeInfraDto>($"api/v1/BranchOffice/{branchOfficeId}");
            if (infra == null)
                throw new InvalidOperationException($"La sucursal {branchOfficeId} no existe.");

            return new BranchOfficeDto
            {
                BranchOfficeId = infra.BranchOfficeId,
                Name = infra.Name
                // mapear otros campos si los necesitas
            };
        }

        public async Task UpdateBranchOffice(Guid vehicleId, int branchOfficeId)
        {
            var response = await _httpClient.PatchAsync(
                $"api/v1/Vehicle/{vehicleId}/branchOffice?branchOfficeId={branchOfficeId}",
                content: null);
            response.EnsureSuccessStatusCode();
        }


        public async Task<VehicleReviewResponse> AddReviewAsync(Guid vehicleId, VehicleReviewRequest req)
        {
            var response = await _httpClient.PatchAsJsonAsync(
                $"api/v1/vehicle/{vehicleId}/reviews", req);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<VehicleReviewResponse>()!;
        }
    }
}



