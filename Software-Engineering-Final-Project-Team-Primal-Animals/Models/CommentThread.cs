using System;
using System.ComponentModel.DataAnnotations;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class CommentThread
    {
        [Key]   
        public int CommentThreadId { get; set; }

        // Foreign key to SensorData
        public int Data_ID { get; set; }
        public SensorData SensorData { get; set; }

        // Identity user who wrote the comment
        public string User_IdentityId { get; set; }

        public string Content { get; set; }
        public DateTime Comment_Time { get; set; }
    }
}
