using Application.Dtos.Response;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices.IReservationServices
{
    public interface IReservationGetService
    {
        Task<ReservationResponse> GetById(Guid reservationId);
        Task<PagedResult<ReservationSummaryResponse>> GetReservationsAsync(
            int userId,
            ReservationStatus? status,
            DateTime? from,
            DateTime? to,
            int? offset,
            int? size);

    }
}
