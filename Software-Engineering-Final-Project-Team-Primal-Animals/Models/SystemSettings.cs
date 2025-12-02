using System.ComponentModel.DataAnnotations;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class SystemSetting
    {
        public int Id { get; set; }

        [Required]
        public string Theme { get; set; } = "light";   // light or Dark

        public bool EmailAlerts { get; set; }
        public bool AnomalyAlerts { get; set; }

        public string RefreshRate { get; set; } = "10";

        public string Timezone { get; set; } = "UTC";
    }
}
