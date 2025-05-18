using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.External
{
    //public class VehicleSummaryDto
    //{
    //    public Guid Id { get; set; }
    //    public string Brand { get; set; }
    //    public string Model { get; set; }
    //    public decimal Price { get; set; }
    //    public string ImageUrl { get; set; }
    //    public int SeatingCapacity { get; set; }
    //    public VehicleCategoryDto Category { get; set; }
    //    public int BranchOfficeId { get; set; }
    //}

    public class VehicleSummaryDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; }          // p. ej. "Toyota"
        public string Model { get; set; }          // p. ej. "Corolla"
        public decimal Price { get; set; }         // tarifa por hora
        public int SeatingCapacity { get; set; }   // cantidad de pasajeros
        public int BranchOfficeId { get; set; }     // id de la sucursal origen
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public string TransmissionType { get; set; }
        public string BranchOfficeName { get; set; } // nombre de la sucursal origen
    }
}
