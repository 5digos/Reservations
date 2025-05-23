using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Request
{
    public class GetReservationsRequest
    {
        public ReservationStatus? Status { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? Offset { get; set; }
        public int? Size { get; set; }
    }
}
