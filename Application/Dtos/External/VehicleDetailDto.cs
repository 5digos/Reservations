using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.External
{
    public class VehicleDetailDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public decimal Price { get; set; }
        public int SeatingCapacity { get; set; }
        public string Color { get; set; }
        public string ImageUrl { get; set; }
        public string CategoryName { get; set; }
        public string TransmissionTypeName { get; set; }
        public int StatusId { get; set; }
        public BranchOfficeDto BranchOffice { get; set; }
        
    }
}
