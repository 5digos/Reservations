using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.External
{
    public class VehicleSummaryDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int SeatingCapacity { get; set; }
        public VehicleCategoryDto Category { get; set; }
        public int BranchOfficeId { get; set; }
    }
}
