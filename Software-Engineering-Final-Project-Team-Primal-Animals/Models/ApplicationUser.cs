using Microsoft.AspNetCore.Identity;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Full_Name { get; set; }
        public string? Account_Status { get; set; }
    }
}
