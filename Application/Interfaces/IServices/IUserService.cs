using System.Threading.Tasks;

namespace Application.Interfaces.IServices
{
    public interface IUserService
    {
        Task<bool> IsUserValidAsync(int userId);
    }
}
