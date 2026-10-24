using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Veterinarian : User
    {
        // JSON array of work shifts; the Application layer owns the format.
        public string? WorkShifts { get; set; }

        public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    }
}
