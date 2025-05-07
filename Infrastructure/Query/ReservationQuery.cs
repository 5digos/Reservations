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

        public async Task<bool> HasOverlap(Guid vehicleId, DateTime start, DateTime end)
        {
            return await _context.Reservations
                .AnyAsync(r => r.VehicleId == vehicleId
                    && r.Status != ReservationStatus.Cancelled
                    && r.Status != ReservationStatus.AutoCancelled
                    && r.StartTime < end
                    && r.EndTime > start);
        }

        public async Task<int?> GetLastReturnBranch(Guid vehicleId, DateTime beforeTime)
        {
            var last = await _context.Reservations
                .Where(r => r.VehicleId == vehicleId && r.EndTime <= beforeTime)
                .OrderByDescending(r => r.EndTime)
                .FirstOrDefaultAsync();

            return last?.DropOffBranchOfficeId;
        }

    }
}
