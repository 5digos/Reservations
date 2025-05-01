using Application.Interfaces.ICommand;
using Application.Interfaces.IServices;
using Domain.Entities;
using System.Threading.Tasks;

namespace Infrastructure.Command
{
    public class CreateReservationCommand : ICreateReservationCommand
    {
        private readonly IReservationRepository _reservationRepository;

        public CreateReservationCommand(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public async Task ExecuteAsync(Reservation reservation)
        {
            await _reservationRepository.AddAsync(reservation);
        }
    }
}
