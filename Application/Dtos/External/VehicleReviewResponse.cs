using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.External
{
    public class VehicleReviewResponse
    {        
        public Guid VehicleId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }        
    }
}
