using Application.Dtos.Response;
using Application.Dtos.Response;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
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

        public async Task<ReservationResponse?> ExecuteAsync(Guid reservationId)
        {
            var reservation = await _repository.GetByIdAsync(reservationId);
            if (reservation == null) return null;

            return new ReservationResponse
            {
                ReservationId = reservation.ReservationId,
                VehicleId = reservation.VehicleId,
                UserId = reservation.UserId,
                PickUpBranchOfficeId = reservation.PickUpBranchOfficeId,
                DropOffBranchOfficeId = reservation.DropOffBranchOfficeId,
                Date = reservation.Date,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                ReservationStatusId = reservation.ReservationStatusId
            };
        }
    }
}
