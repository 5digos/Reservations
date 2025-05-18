using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Response
{
    public class VehicleSummaryResponse
    {
        public Guid Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int SeatingCapacity { get; set; }
        public string TransmissionType { get; set; }
        public string Category { get; set; }
        //public VehicleCategoryResponse Category { get; set; }
        //public int BranchOfficeId { get; set; }
        //public string BranchOfficeName { get; set; }

    }

}
