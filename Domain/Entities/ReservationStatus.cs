﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ReservationStatus
    {
        public int ReservationStatusId { get; set; }
        public string Name { get; set; }

        public ICollection<Reservation> Reservations { get; set; }
    }
}
