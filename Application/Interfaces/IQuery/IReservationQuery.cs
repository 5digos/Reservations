using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IQuery
{
    public interface IReservationQuery
    {
        Task<bool> HasOverlap(Guid vehicleId, DateTime start, DateTime end, int bufferHours);
        Task<Reservation> GetById(Guid reservationId);
        Task<int?> GetLastReturnBranch(Guid vehicleId, DateTime beforeTime);
        Task<int?> GetNextPickupBranch(Guid vehicleId, DateTime afterTime);
        Task<(List<Reservation> Reservations, int TotalCount)>GetReservationsAsync(
                int userId,
                ReservationStatus? status,
                DateTime? from,
                DateTime? to,
                int? offset,
                int? size);
    }
}
