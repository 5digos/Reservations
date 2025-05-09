using Application.Dtos.Response;
using Application.Dtos.Response;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<ReservationResponse>> ExecuteAsync()
        {
            var list = await _repository.GetAllAsync();

            return list.Select(r => new ReservationResponse
            {
                ReservationId = r.ReservationId,
                VehicleId = r.VehicleId,
                UserId = r.UserId,
                PickUpBranchOfficeId = r.PickUpBranchOfficeId,
                DropOffBranchOfficeId = r.DropOffBranchOfficeId,
                Date = r.Date,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                ReservationStatusId = r.ReservationStatusId
            }).ToList();
        }
    }
}
