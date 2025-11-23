using System;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class Report
    {
        public int id { get; set; }

        // Link to the Patient/User the report is about
        public string user_id { get; set; } = null!;
       
        public Users? user { get; set; }

        // Optional: Link to the Clinician who may have triggered the report
        public string? generated_by_clinician_id { get; set; } // Nullable, as reports might be auto-generated
       
        public Users? generated_by_clinician { get; set; }

        // Type of report (e.g., "Daily Summary", "Weekly Comparison", "Alert Review")
        public string report_type { get; set; } = string.Empty;

        // Stores the actual content/data of the report (e.g., CSV, HTML, or JSON structure)
        public string report_data_json { get; set; } = string.Empty;

        // The time period the report covers (e.g., "Last 24h", "Last 7 days")
        public string time_period_covered { get; set; } = string.Empty;

        public DateTime generated_at { get; set; } = DateTime.UtcNow;
    }
}