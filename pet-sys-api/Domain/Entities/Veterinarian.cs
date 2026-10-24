using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Veterinarian : User
    {
        // Serialized to a single JSON column; nullable to match the TPH table, where
        // non-veterinarian rows have no value.
        public List<WorkShift>? WorkShifts { get; set; }

        public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    }
}
