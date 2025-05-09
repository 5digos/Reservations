using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }



        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationStatus> ReservationStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(r => r.ReservationId);

                entity.Property(r => r.Date).HasColumnType("date");
                entity.Property(r => r.StartTime).IsRequired();
                entity.Property(r => r.EndTime).IsRequired();

                
                entity.HasOne(r => r.ReservationStatus)
                      .WithMany(s => s.Reservations)
                      .HasForeignKey(r => r.ReservationStatusId);
            });

            modelBuilder.Entity<ReservationStatus>(entity =>
            {
                entity.HasKey(rs => rs.ReservationStatusId);
                entity.Property(rs => rs.Name).IsRequired().HasMaxLength(25);
            });
            modelBuilder.Entity<ReservationStatus>().HasData(
            new ReservationStatus { ReservationStatusId = 1, Name = "Pending" },
            new ReservationStatus { ReservationStatusId = 2, Name = "Confirmed" },
            new ReservationStatus { ReservationStatusId = 3, Name = "Cancelled" }
    );
        }
    }
}