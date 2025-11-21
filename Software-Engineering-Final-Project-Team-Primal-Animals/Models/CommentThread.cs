namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class CommentThread
    {
        public int Comment_ID { get; set; }

        public int User_ID { get; set; }   // Patient user ID
        public AppUser User { get; set; }

        public int Data_ID { get; set; }
        public SensorData SensorData { get; set; }

        public string Content { get; set; }
        public DateTime Comment_Time { get; set; }
    }
}
