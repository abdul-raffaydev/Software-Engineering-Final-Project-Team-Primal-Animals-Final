using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class Patient
    {
        [Key]   // REQUIRED so EF knows this is the primary key
        public int Patient_ID { get; set; }

        public string Full_Name { get; set; }
        public string Emergency_contactName { get; set; }
        public int Emergency_ContactNumber { get; set; }
        public string Age { get; set; }
        public string DateOfBirth { get; set; }

        // FK to AppUser
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public ICollection<SensorData> SensorData { get; set; } = new List<SensorData>();
        public ICollection<CommentThread> Comments { get; set; } = new List<CommentThread>();
    }
}


