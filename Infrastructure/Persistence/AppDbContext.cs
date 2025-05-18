using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationEvent> ReservationEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.ToTable("Reservations");
                entity.HasKey(r => r.ReservationId);
                entity.Property(r => r.ReservationId)
                    .ValueGeneratedOnAdd();

                entity.HasMany(r => r.Events)
                    .WithOne(e => e.Reservation)
                    .HasForeignKey(e => e.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(r => r.UserId)
                    .IsRequired();

                entity.Property(r => r.VehicleId)
                    .IsRequired();

                entity.Property(r => r.PickupBranchOfficeId)
                    .IsRequired();

                entity.Property(r => r.DropOffBranchOfficeId)
                    .IsRequired();

                entity.Property(r => r.StartTime)
                    .IsRequired();

                entity.Property(r => r.EndTime)
                    .IsRequired();

                entity.Property(r => r.HourlyRateSnapshot)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(r => r.OriginalCost)
                    .IsRequired(false)
                    .HasColumnType("decimal(10,2)");

                entity.Property(r => r.LateFee)
                    .IsRequired(false)
                    .HasColumnType("decimal(10,2)");

                entity.Property(r => r.Status)
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString(),
                        v => (ReservationStatus)Enum.Parse(typeof(ReservationStatus), v));

                entity.Property(r => r.ActualPickupTime)
                    .IsRequired(false);

                entity.Property(r => r.ActualReturnTime)
                    .IsRequired(false);
            });

            modelBuilder.Entity<ReservationEvent>(entity =>
            {
                entity.ToTable("ReservationEvents");
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ReservationId)
                    .IsRequired();

                entity.Property(e => e.EventType)
                    .IsRequired()
                    .HasConversion(
                        v => v.ToString(),
                        v => (ReservationEventType)Enum.Parse(typeof(ReservationEventType), v));

                entity.Property(e => e.OccurredAt)
                    .IsRequired();

                entity.Property(e => e.Details)
                    .HasColumnType("varchar(max)")
                    .IsRequired(false);
            });


        }
    }
}