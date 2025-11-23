using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;


namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class UserRoleMap
    {
        public int id { get; set; }

        public string user_id { get; set; } = null!;
        public Users? user { get; set; }

        public string role_id { get; set; } = null!;
        public Roles? role { get; set; }
    }
}
