using System;
using System.ComponentModel.DataAnnotations;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class CommentThread
    {
        [Key]
        public int CommentThread_ID { get; set; }

        public int Patient_ID { get; set; }
        public Patient? Patient { get; set; }

        // ✅ REQUIRED (controllers use this)
        public int Data_ID { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime Comment_Time { get; set; }

        public string AuthorRole { get; set; } = string.Empty;
    }
}
