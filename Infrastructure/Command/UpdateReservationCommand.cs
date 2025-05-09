using Application.Dtos.Request;
using Application.Dtos.Request;
using Application.Interfaces.ICommand;
using Application.Interfaces.IServices;
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

        public async Task ExecuteAsync(Guid id, UpdateReservationRequest updatedReservation)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Reservation not found.");

            // Actualizar solo los campos permitidos
            existing.VehicleId = updatedReservation.VehicleId;
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
