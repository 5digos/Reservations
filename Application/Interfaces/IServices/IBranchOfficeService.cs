using System.Threading.Tasks;

namespace Application.Interfaces.IServices
{
    public interface IBranchOfficeService
    {
        Task<bool> IsBranchOfficeValidAsync(int branchOfficeId);
    }
}
