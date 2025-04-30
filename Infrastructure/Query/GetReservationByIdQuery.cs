using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Query
{
    public class GetReservationByIdQuery : IGetReservationByIdQuery
    {
        private readonly IReservationRepository _repository;

        public GetReservationByIdQuery(IReservationRepository repository)
        {
            _repository = repository;
        }

        public async Task<Reservation?> ExecuteAsync(Guid reservationId)
        {
            return await _repository.GetByIdAsync(reservationId);
        }
    }
}
