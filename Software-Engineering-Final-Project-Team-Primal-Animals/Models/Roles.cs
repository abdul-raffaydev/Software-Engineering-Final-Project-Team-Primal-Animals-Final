using System;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class Roles
    {
        public string role_id { get; set; } = null!;
        public string description { get; set; } = string.Empty;

        public ICollection<UserRoleMap> user_role_map { get; set; } = new List<UserRoleMap>();
    }
}
