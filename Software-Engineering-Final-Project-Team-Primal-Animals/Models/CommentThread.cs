namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class CommentThread
    {
        public int CommentThreadId { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public DateTime FrameTimestamp { get; set; }

        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; }

        public string ClinicianReply { get; set; }
        public DateTime? ClinicianReplyAt { get; set; }
    }
}
