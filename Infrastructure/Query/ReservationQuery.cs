using Application.Interfaces.IQuery;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Query
{
    public class ReservationQuery : IReservationQuery
    {
        private readonly AppDbContext _context;

        public ReservationQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Reservation> GetById(Guid reservationId)
        {
            return await _context.Reservations
                .Include(r => r.Events)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
        }


        //public async Task<bool> HasOverlap(Guid vehicleId, DateTime start, DateTime end, int bufferHours = 3)
        //{
        //    // en lugar de r.EndTime.Add(buffer) > start, precomputamos:
        //    var bufferedStart = start - TimeSpan.FromHours(bufferHours);

        //    return await _context.Reservations
        //        .AnyAsync(r =>
        //            r.VehicleId == vehicleId
        //            && r.Status != ReservationStatus.Cancelled
        //            && r.Status != ReservationStatus.AutoCancelled
        //            // el EndTime original debe ser posterior a (start - buffer)
        //            && r.EndTime > bufferedStart
        //            && r.StartTime < end
        //        );
        //}

        public async Task<bool> HasOverlap(Guid vehicleId, DateTime start, DateTime end, int bufferHours = 3)
        {
            // Extendemos el intervalo por ambos lados:
            var bufferedStart = start.AddHours(-bufferHours);
            var bufferedEnd = end.AddHours(bufferHours);

            return await _context.Reservations
                .AnyAsync(r =>
                    r.VehicleId == vehicleId
                    && r.Status != ReservationStatus.Cancelled
                    && r.Status != ReservationStatus.AutoCancelled
                    // chequear que haya ANY overlap contra [bufferedStart, bufferedEnd]
                    && r.StartTime < bufferedEnd
                    && r.EndTime > bufferedStart
                );
        }

        public async Task<int?> GetLastReturnBranch(Guid vehicleId, DateTime beforeTime)
        {
            var last = await _context.Reservations
                .Where(r => r.VehicleId == vehicleId && r.EndTime <= beforeTime)
                .OrderByDescending(r => r.EndTime)
                .FirstOrDefaultAsync();

            return last?.DropOffBranchOfficeId;
        }

        public async Task<int?> GetNextPickupBranch(Guid vehicleId, DateTime afterTime)
        {
            var next = await _context.Reservations
                .Where(r => r.VehicleId == vehicleId
                            && r.StartTime >= afterTime
                            && r.Status != ReservationStatus.Cancelled
                            && r.Status != ReservationStatus.AutoCancelled)
                .OrderBy(r => r.StartTime)
                .FirstOrDefaultAsync();

            return next?.PickupBranchOfficeId;
        }
    }
}
