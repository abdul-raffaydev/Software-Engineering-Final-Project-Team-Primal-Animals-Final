namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class Patient
   { public int Patient_ID { get; set; }
    public string Full_Name { get; set; }
    public string Emergency_contactName { get; set; }
    public int Emergency_ContactNumber { get; set; }
    public string Age { get; set; }
    public string DateOfBirth { get; set; }

    // FK → User
    public int User_ID { get; set; }
    public AppUser User { get; set; }

    // Relationships
    public ICollection<SensorData> SensorData { get; set; }
    public ICollection<CommentThread> Comments { get; set; }
}
}
