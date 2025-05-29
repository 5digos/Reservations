using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Reservation
    {
        public Guid ReservationId { get; set; }
        public int UserId { get; set; }
        public Guid VehicleId { get; set; }
        public int PickupBranchOfficeId { get; set; }
        public int DropOffBranchOfficeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? ActualPickupTime { get; set; }
        public DateTime? ActualReturnTime { get; set; }        
        public decimal HourlyRateSnapshot { get; set; }  // Snapshot de tarifa        
        public ReservationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<ReservationEvent> Events { get; set; }
    }
}
