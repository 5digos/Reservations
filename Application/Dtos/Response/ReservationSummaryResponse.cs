using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Response
{
    public class ReservationSummaryResponse
    {
        public Guid ReservationId { get; set; }        
        public Guid VehicleId { get; set; }        
        public string PickupBranchOfficeName { get; set; }        
        public string DropOffBranchOfficeName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ReservationStatus Status { get; set; }
    }
}
