using System.ComponentModel.DataAnnotations;

namespace Software_Engineering_Project.Models
{
    [cite_start]// This enum defines the roles from the case study [cite: 13]
    public enum UserRole
    {
        Admin,
        Clinician,
        Patient // The 'user' login
    }

    public class ApplicationUser
    {
        [Key] // Tells the database this is the primary key
        public int UserId { get; set; }

        [Required] // Makes this field mandatory
        public string Name { get; set; }

        [Required]
        [EmailAddress] // Adds validation for email format
        public string Email { get; set; }

        [Required]
        public string HashedPassword { get; set; } // We will store a HASH, not the plain password

        [Required]
        public UserRole Role { get; set; }
    }
}