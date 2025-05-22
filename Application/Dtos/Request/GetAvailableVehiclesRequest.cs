using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Request
{
    public class GetAvailableVehiclesRequest
    {
        public int PickupBranchOfficeId { get; set; }
        public int DropOffBranchOfficeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Paginación
        public int? Offset { get; set; } = 0;
        public int? Size { get; set; } = 1000;

        // Filtros opcionales
        public int? Category { get; set; }
        public int? SeatingCapacity { get; set; }
        public int? TransmissionType { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Color { get; set; }
        public string? Brand { get; set; }
    }
}
