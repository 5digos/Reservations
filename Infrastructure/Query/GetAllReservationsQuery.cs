using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Query
{
    public class GetAllReservationsQuery : IGetAllReservationsQuery
    {
        private readonly IReservationRepository _repository;

        public GetAllReservationsQuery(IReservationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Reservation>> ExecuteAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
