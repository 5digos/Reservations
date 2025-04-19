using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    internal class BranchOffice
    {
        public int BranchOfficeId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public ICollection<Reservation> PickUps { get; set; }
        public ICollection<Reservation> DropOffs { get; set; }
    }
}
