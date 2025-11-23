using System;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;


namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class Alert
    {
        public int id { get; set; }

        public string user_id { get; set; } = null!;
        public Users? User { get; set; }

        public int sensor_frame_id { get; set; }
        public SensorFrame? SensorFrame { get; set; }

        public string message { get; set; } = string.Empty;

        public bool is_reviewed_by_clinician { get; set; }

        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
