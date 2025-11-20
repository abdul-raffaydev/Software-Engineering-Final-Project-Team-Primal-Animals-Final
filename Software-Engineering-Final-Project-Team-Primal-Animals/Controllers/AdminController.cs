using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Project.Data; 
using Software_Engineering_Project.Models;
using System.Threading.Tasks;

namespace Software_Engineering_Project.Controllers
{
    public class AdminController : Controller
    {
        // DbContext to interact with the database
        private readonly ApplicationDbContext _context;

        // Constructor to initialize the DbContext
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // This function handles creating Admins, Clinicians, and Patients
        [HttpPost]
        public async Task<IActionResult> CreateUser(string name, string email, string password, UserRole role)
        {
            // Check if the email is unique
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email address is already in use.");
                return View(); // Return to the form with an error message
            }
            // Hash the password before storing it

            string hashedPassword = "hashed_" + password;

            var newUser = new ApplicationUser
            {
                Name = name,
                Email = email,
                HashedPassword = hashedPassword,
                Role = role // This sets the user type
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(); // Save to database

            return RedirectToAction("UserManagementDashboard");
        }

        //Delete User Accounts
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserManagementDashboard");
        }

        // A page to show all users
        public async Task<IActionResult> UserManagementDashboard()
        {
            var allUsers = await _context.Users.ToListAsync();
            return View(allUsers); // Send the list of users to the HTML page
        }
    }
}