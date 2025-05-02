using System;
using System.Threading.Tasks;

namespace Application.Interfaces.ICommand
{
    public interface IDeleteReservationCommand
    {
        Task ExecuteAsync(Guid id);
    }
}
