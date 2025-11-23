using System;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class UserClinicianAssignment
    {
        public int id { get; set; }

        public string clinician_id { get; set; } = null!;
        public Users? clinician { get; set; }

        public string patient_id { get; set; } = null!;
        public Users? patient { get; set; }

        public DateTime assigned_on { get; set; } = DateTime.UtcNow;
    }
}
