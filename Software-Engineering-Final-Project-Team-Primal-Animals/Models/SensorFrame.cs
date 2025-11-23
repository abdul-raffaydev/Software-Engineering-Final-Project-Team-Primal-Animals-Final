using System;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class SensorFrame
    {
        public int id { get; set; }

        public string user_id { get; set; } = null!;
        public Users? user { get; set; }

        public DateTime time_stamp { get; set; }

        public string pressure_values_json { get; set; } = string.Empty;

        public ICollection<UserComment> UserComments { get; set; } = new List<UserComment>();
        public ICollection<MetricRecord> MetricRecords { get; set; } = new List<MetricRecord>();
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
