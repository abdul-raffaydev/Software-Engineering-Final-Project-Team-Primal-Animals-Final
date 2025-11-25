using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

public class Patient
{
    public int Patient_ID { get; set; }
    public string Full_Name { get; set; }
    public string Emergency_contactName { get; set; }
    public int Emergency_ContactNumber { get; set; }
    public string Age { get; set; }
    public string DateOfBirth { get; set; }

    public string AppUserId { get; set; }
    public ApplicationUser AppUser { get; set; }
    public int HighPressureThreshold { get; set; } = 180;

    public ICollection<SensorData> SensorData { get; set; } = new List<SensorData>();
    public ICollection<CommentThread> Comments { get; set; } = new List<CommentThread>();
}



