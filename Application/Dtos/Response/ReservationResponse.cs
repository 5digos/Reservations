using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Response
{
    public class ReservationResponse
    {
        public Guid ReservationId { get; set; }
        public int UserId { get; set; }
        public Guid VehicleId { get; set; }
        public int PickupBranchOfficeId { get; set; }
        public string PickupBranchOfficeName { get; set; }
        public int DropOffBranchOfficeId { get; set; }
        public string DropOffBranchOfficeName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal HourlyRateSnapshot { get; set; }
    }
}
