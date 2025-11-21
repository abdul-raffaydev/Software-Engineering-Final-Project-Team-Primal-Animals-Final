using System.ComponentModel.DataAnnotations;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class AppUser
    {
        [Key]  
        public int AppUserId { get; set; }

        public string User_Email { get; set; }
        public string Password_Hash { get; set; }
        public string Full_Name { get; set; }
        public string Account_Status { get; set; }

        // 1-to-1 with Patient
        public Patient Patient { get; set; }
    }
}
