using System;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class CommentThread
    {
        public int Comment_ID { get; set; }

        public int Patient_ID { get; set; }        //  FK to Patient
        public int Data_ID { get; set; }           // FK to SensorData

        public string Content { get; set; }
        public DateTime Comment_Time { get; set; }

        // ✅ Navigation Property (REQUIRED)
        public Patient Patient { get; set; }
    }
}
