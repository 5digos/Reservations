using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ReservationEvent
    {
        public Guid EventId { get; set; }
        public Guid ReservationId { get; set; }
        public ReservationEventType EventType { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Details { get; set; }

        
        public Reservation Reservation { get; set; }
    }
}
