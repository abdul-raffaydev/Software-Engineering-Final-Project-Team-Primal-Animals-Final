namespace Software_Engineering_Final_Project_Team_Primal_Animals.ViewModels
{
    public class ClinicianDashboardVM
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }

        public string Status { get; set; }          // e.g. "OK", "High Pressure Alert"
        public string Cue { get; set; }             // short description of reason
        public string LastActivity { get; set; }    // e.g. "Data synced 3m ago"
    }
}
