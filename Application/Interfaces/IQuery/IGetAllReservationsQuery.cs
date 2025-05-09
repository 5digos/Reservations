using Application.Dtos.Response;
using Application.Dtos.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.IQuery
{
    public interface IGetAllReservationsQuery
    {
        Task<List<ReservationResponse>> ExecuteAsync();
    }
}
