using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IQuery
{
    public interface IReservationQuery
    {
        Task<bool> HasOverlap(Guid vehicleId, DateTime start, DateTime end);
        Task<Reservation> GetById(Guid reservationId);
        Task<int?> GetLastReturnBranch(Guid vehicleId, DateTime beforeTime);
    }
}
