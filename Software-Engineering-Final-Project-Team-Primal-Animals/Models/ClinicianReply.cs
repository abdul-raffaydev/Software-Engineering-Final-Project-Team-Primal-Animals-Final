using System;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class ClinicianReply
    {
        public int id { get; set; }

        public int comment_id { get; set; }
        public UserComment? comment { get; set; }

        public string clinician_id { get; set; } = null!;
        public Users? clinician { get; set; }

        public string reply_text { get; set; } = string.Empty;

        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
