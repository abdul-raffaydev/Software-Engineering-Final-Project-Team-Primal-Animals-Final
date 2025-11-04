namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class Patient
    { public int Id { get; set; }
      public string Name { get; set; }
      public int Age { get; set; }  

      public ICollection<PressureFrame> PressureFrames { get; set; }

        public ICollection<CommentThread> Comments { get; set; }

        public string EmergencyContactName { get; set; }
        public string EmergencyContactNumber { get; set; }

    }
}
