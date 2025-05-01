using Application.Dtos.Request;
using Application.Dtos.Response;
using System.Threading.Tasks;

namespace Application.Interfaces.ICommand
{
    public interface ICreateReservationCommand
    {
        Task<ReservationResponse> ExecuteAsync(CreateReservationRequest request);
    }
}
