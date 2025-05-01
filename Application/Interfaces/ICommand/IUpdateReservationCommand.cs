using Application.Dtos.Request;
using Application.Dtos.Request;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.ICommand
{
    public interface IUpdateReservationCommand
    {
        Task ExecuteAsync(Guid id, UpdateReservationRequest updatedReservation);
    }
}