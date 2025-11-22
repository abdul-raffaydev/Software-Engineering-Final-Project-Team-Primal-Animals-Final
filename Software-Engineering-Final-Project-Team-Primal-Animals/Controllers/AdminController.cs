using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;
using System.Security.Cryptography;
using System.Text;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard
        public IActionResult AdminDashboard()
        {
            return View();
        }

        // List Users
        public async Task<IActionResult> UserManagement()
        {
            var users = await _context.UserAccounts.ToListAsync();
            return View(users);
        }

        // Create Account (OLD WORKING BEHAVIOR RESTORED)
        [HttpPost]
        public async Task<IActionResult> CreateAccount(string Username, string Email, string Password, string Role)
        {
            // Check if email already exists in UserAccounts (not Identity)
            if (_context.UserAccounts.Any(u => u.Email == Email))
            {
                TempData["error"] = "Email already exists!";
                return RedirectToAction("UserManagement");
            }

            var newUser = new UserAccount
            {
                Username = Username,
                Email = Email,
                PasswordHash = HashPassword(Password),
                Role = Role
            };

            _context.UserAccounts.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["success"] = "Account created successfully!";
            return RedirectToAction("UserManagement");
        }

        // Reset Password (OLD VERSION)
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.UserAccounts.FindAsync(id);
            if (user == null)
                return NotFound();

            string defaultPassword = "Password123!";
            user.PasswordHash = HashPassword(defaultPassword);

            await _context.SaveChangesAsync();

            TempData["success"] = "Password reset to: Password123!";
            return RedirectToAction("UserManagement");
        }

        // Delete Account
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var user = await _context.UserAccounts.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.UserAccounts.Remove(user);
            await _context.SaveChangesAsync();

            TempData["success"] = "User deleted!";
            return RedirectToAction("UserManagement");
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
