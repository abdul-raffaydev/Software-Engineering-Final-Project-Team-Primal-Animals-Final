namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class AppUser
    {
        public int User_ID { get; set; }
        public string User_Email { get; set; }
        public string Password_Hash { get; set; }
        public string Full_Name { get; set; }
        public string Account_Status { get; set; }

        // Relationships
        public Patient Patient { get; set; }
    
        public ICollection<CommentThread> Comments { get; set; }
    }
}
