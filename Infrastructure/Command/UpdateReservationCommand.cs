using Application.Interfaces.ICommand;
using Application.Interfaces.IServices;
using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Command
{
    public class UpdateReservationCommand : IUpdateReservationCommand
    {
        private readonly IReservationRepository _repository;

        public UpdateReservationCommand(IReservationRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(Guid id, Reservation updatedReservation)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Reservation not found.");

            // Actualizamos los campos deseados
            existing.VehicleId = updatedReservation.VehicleId;
            existing.ReservationStatusId = updatedReservation.ReservationStatusId;
            existing.UserId = updatedReservation.UserId;
            existing.PickUpBranchOfficeId = updatedReservation.PickUpBranchOfficeId;
            existing.DropOffBranchOfficeId = updatedReservation.DropOffBranchOfficeId;
            existing.Date = updatedReservation.Date;
            existing.StartTime = updatedReservation.StartTime;
            existing.EndTime = updatedReservation.EndTime;

            await _repository.UpdateAsync(existing);
        }
    }
}
