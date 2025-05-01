using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.IQuery
{
    public interface IGetAllReservationsQuery
    {
        Task<List<Reservation>> ExecuteAsync();
    }
}
