using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Interfaces.ICommand
{
    public interface ICreateReservationCommand
    {
        Task ExecuteAsync(Reservation reservation);
    }
}
