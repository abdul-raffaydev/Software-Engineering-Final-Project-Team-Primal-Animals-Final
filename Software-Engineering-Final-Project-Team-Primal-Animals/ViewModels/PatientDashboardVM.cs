using System;
using System.Collections.Generic;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.ViewModels
{
    public class PatientDashboardVM
    {
        public string PatientName { get; set; }

        public string PressureMatrix { get; set; }
        public int PeakPressure { get; set; }
        public string ContactArea { get; set; }
        public DateTime Timestamp { get; set; }
        public int DataId { get; set; }

        public string EmergencyName { get; set; }
        public int EmergencyNumber { get; set; }

        public bool IsHighRisk { get; set; }
        public string AlertMessage { get; set; }

        // Trend Graph
        public List<string> TrendLabels { get; set; } = new();
        public List<int> TrendValues { get; set; } = new();
    }
}

