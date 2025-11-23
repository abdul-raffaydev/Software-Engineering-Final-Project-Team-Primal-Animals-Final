using System;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class UserComment
    {
        public int id { get; set; }

        public string user_id { get; set; } = null!;
        public Users? user { get; set; }

        public int sensor_frame_id { get; set; }
        public SensorFrame? sensor_frame { get; set; }

        public string comment_text { get; set; } = string.Empty;

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public ICollection<ClinicianReply> replies { get; set; } = new List<ClinicianReply>();
    }
}
