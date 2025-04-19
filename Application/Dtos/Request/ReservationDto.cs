using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Request
{
    class ReservationDto
    {
        public Guid VehicleId { get; set; }
        public int UserId { get; set; }
        public int PickUpBranchOfficeId { get; set; }
        public int DropOffBranchOfficeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
