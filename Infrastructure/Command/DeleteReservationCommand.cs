using Application.Interfaces.ICommand;
using Application.Interfaces.IServices;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Command
{
    public class DeleteReservationCommand : IDeleteReservationCommand
    {
        private readonly IReservationRepository _repository;

        public DeleteReservationCommand(IReservationRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}

