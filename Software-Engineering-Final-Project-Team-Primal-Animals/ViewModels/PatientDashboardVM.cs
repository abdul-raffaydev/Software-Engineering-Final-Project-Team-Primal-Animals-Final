namespace Software_Engineering_Final_Project_Team_Primal_Animals.ViewModels;    

public class PatientDashboardVM
{
    public string PressureMatrix { get; set; }
    public int PeakPressure { get; set; }
    public string ContactArea { get; set; }

    public string EmergencyName { get; set; }
    public int EmergencyNumber { get; set; }
    public int DataId { get; set; }
    public DateTime Timestamp { get; set; }
}

