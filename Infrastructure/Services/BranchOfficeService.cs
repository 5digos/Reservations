using Application.Interfaces.IServices;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class BranchOfficeService : IBranchOfficeService
    {
        private readonly HttpClient _http;

        public BranchOfficeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> IsBranchOfficeValidAsync(int branchOfficeId)
        {
            //Cuando tengamos BranchOfficeMs
            //var response = await _http.GetAsync($"/api/branchoffices/{branchOfficeId}");
            //return response.IsSuccessStatusCode;

            // Simulación temporal:
            await Task.Delay(10);
            return true;
        }
    }
}
