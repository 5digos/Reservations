using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.IQuery
{
    public interface IGetReservationByIdQuery
    {
        Task<Reservation?> ExecuteAsync(Guid reservationId);
    }
}
