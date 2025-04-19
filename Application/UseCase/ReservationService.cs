using Application.Dtos.Request;
using Application.Interfaces.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase
{
    class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;
        //pasarle la base de datos
        public ReservationService(AppDbContext context)
        {
            _context = context;
        }
        //crea la reserva pasandole los datos necesarios
        public async Task<Reservation> CreateReservationAsync(ReservationDto dto)
        {
            //variable de reserva
            var reservation = new Reservation
            {
                ReservationId = Guid.NewGuid(),
                UserId = dto.UserId,
                VehicleId = dto.VehicleId,
                PickUpBranchOfficeId = dto.PickUpBranchOfficeId,
                DropOffBranchOfficeId = dto.DropOffBranchOfficeId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                ReservationStatusId = 1 // estado inicial "Pendiente", por ejemplo
            };
            //guardarla en la base de datos
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            //regresar la reserva
            return reservation;
        }
    }
}
