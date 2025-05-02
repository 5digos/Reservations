using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Request
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string Plate { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
        public int OwnerId { get; set; }
    }
}
