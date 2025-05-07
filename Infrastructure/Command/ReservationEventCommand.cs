using Application.Interfaces.ICommand;
using Domain.Entities;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Command
{
    public class ReservationEventCommand : IReservationEventCommand
    {
        private readonly AppDbContext _context;

        public ReservationEventCommand(AppDbContext context)
        {
            _context = context;
        }

        public async Task Add(ReservationEvent reservationEvent)
        {
            _context.ReservationEvents.Add(reservationEvent);
            await _context.SaveChangesAsync();
        }
    }
}
