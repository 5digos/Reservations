using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HttpClients.Dtos
{
    public class VehicleResponseInfraDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public decimal Price { get; set; }
        public int SeatingCapacity { get; set; }
        public string Color { get; set; }
        public string ImageUrl { get; set; }
        public GenericInfraDto Status { get; set; }
        public GenericInfraDto TransmissionType { get; set; }
        public VehicleCategoryInfraDto Category { get; set; }
        public BranchOfficeInfraDto BranchOffice { get; set; }
    }
}
