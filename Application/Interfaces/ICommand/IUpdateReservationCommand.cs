using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.ICommand
{
    public interface IUpdateReservationCommand
    {
        Task ExecuteAsync(Guid id, Reservation updatedReservation);
    }
}

