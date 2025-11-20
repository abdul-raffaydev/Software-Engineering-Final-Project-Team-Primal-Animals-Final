namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class SensorData
    {
        public int Data_Id { get; set; }

        public int Patient_ID { get; set; }
        public Patient Patient { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Pressure_Matrix { get; set; } // 32x32 matrix
        public int PeakPressureIndex { get; set; }
        public string Contact_Area { get; set; }
    }
}
