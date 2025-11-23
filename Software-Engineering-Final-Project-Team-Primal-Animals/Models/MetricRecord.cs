using System;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class MetricRecord
    {
        public int id { get; set; }

        public string user_id { get; set; } = null!;
        public Users? user { get; set; }

        public int sensor_frame_id { get; set; }
        public SensorFrame? frame { get; set; }

        public DateTime time_stamp { get; set; }
        public int peak_pressure_index { get; set; }
        public double contact_area_percentage { get; set; }

        public string metrics_json { get; set; } = string.Empty;
    }
}
