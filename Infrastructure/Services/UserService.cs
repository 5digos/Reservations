using Application.Interfaces.IServices;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> IsUserValidAsync(int userId)
        {
            //Cuando tengamos UserMs
            //var response = await _httpClient.GetAsync($"/api/users/{userId}");
            //return response.IsSuccessStatusCode;

            // Simulación temporal:
            await Task.Delay(10);
            return true;
        }
    }
}
