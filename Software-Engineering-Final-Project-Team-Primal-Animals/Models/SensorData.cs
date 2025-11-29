using System;
using System.ComponentModel.DataAnnotations;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class SensorData
    {
        [Key] 
        public int Data_Id { get; set; }

        // Foreign key to Patient
        public int Patient_ID { get; set; }
        public Patient Patient { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Pressure_Matrix { get; set; } // 32x32 matrix
        public int PeakPressureIndex { get; set; }
        public string Contact_Area { get; set; }
    }
}
