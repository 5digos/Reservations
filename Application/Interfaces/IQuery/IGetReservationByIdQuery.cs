using Application.Dtos.Response;
using Application.Dtos.Response;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.IQuery
{
    public interface IGetReservationByIdQuery
    {
        Task<ReservationResponse?> ExecuteAsync(Guid reservationId);
    }
}
