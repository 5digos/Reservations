using Application.Dtos.Request;
using Application.Interfaces.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HttpClients
{
    public class NotificationServiceClient : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationServiceClient> _logger;

        public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task EnqueueEvent(NotificationEventRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/notifications/events", request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Notification event enqueued: {UserId}-{EventType}", request.UserId, request.EventType);
                return;
            }
            _logger.LogError("Error enqueuing notification: {StatusCode}", response.StatusCode);
            response.EnsureSuccessStatusCode();
        }
    }
}
