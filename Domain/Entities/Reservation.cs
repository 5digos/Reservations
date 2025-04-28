using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Reservation
    {
        public Guid ReservationId { get; set; }
        public Guid VehicleId { get; set; }
        public int ReservationStatusId { get; set; }
        public int UserId { get; set; }
        public int PickUpBranchOfficeId { get; set; }
        public int DropOffBranchOfficeId { get; set; }
        public DateOnly Date {  get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Navigation
        public User User { get; set; }
        public Vehicle Vehicle { get; set; }
        public BranchOffice PickUpBranchOffice { get; set; }
        public BranchOffice DropOffBranchOffice { get; set; }
        public ReservationStatus ReservationStatus { get; set; }
    }
}
