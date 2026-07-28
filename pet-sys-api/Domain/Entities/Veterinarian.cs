using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Veterinarian : User
    {
        public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    }
}
