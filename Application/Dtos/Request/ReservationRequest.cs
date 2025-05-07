using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Request
{
    public class ReservationRequest
    {        
        public Guid VehicleId { get; set; }
        public int PickupBranchOfficeId { get; set; }
        public int DropOffBranchOfficeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}

