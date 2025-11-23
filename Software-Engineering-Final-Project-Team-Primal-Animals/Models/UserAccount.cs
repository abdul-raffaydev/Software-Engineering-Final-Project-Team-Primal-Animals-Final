using System;
using System.ComponentModel.DataAnnotations;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Models
{
    public class UserAccount
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }  // Admin / Clinician / Patient

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
