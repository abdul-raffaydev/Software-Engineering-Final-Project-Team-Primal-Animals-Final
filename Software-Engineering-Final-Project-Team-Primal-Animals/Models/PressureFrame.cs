namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class PressureFrame
    {
        public int PressureFrameId { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public string FrameData { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
