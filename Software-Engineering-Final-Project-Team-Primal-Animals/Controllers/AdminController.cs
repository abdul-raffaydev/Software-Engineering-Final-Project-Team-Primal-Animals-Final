using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;
using Software_Engineering_Final_Project_Team_Primal_Animals.ViewModels;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // -------------------------
        // ADMIN DASHBOARD
        // -------------------------
        public IActionResult AdminDashboard()
        {
            return View();
        }

        // -------------------------
        // USER MANAGEMENT
        // -------------------------
        public async Task<IActionResult> UserManagement()
        {
            var users = await _userManager.Users.ToListAsync();
            var list = new List<AdminUserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                list.Add(new AdminUserViewModel
                {
                    Id = user.Id,

                    // FIX: guarantee full name always exists
                    FullName = string.IsNullOrWhiteSpace(user.Full_Name)
                                ? user.Email.Split('@')[0]    // fallback
                                : user.Full_Name,

                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "No role"
                });
            }

            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string name, string email, string password, string role)
        {
            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Full_Name = name
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                TempData["success"] = "User created!";
            }
            else
            {
                TempData["error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("UserManagement");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
                await _userManager.DeleteAsync(user);

            return RedirectToAction("UserManagement");
        }

        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return RedirectToAction("UserManagement");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string newPass = "Password123!";

            await _userManager.ResetPasswordAsync(user, token, newPass);

            TempData["success"] = $"Password reset to {newPass}";
            return RedirectToAction("UserManagement");
        }

        // -------------------------
        // PATIENT VIEW
        // -------------------------
        public IActionResult Patients()
        {
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "GTLB-Data");

            if (!Directory.Exists(basePath))
                return View(new List<CSVPatientViewModel>());

            // Patient name mapping
            var nameMap = new Dictionary<string, string>
            {
                { "1c0fd777", "Hassan" },
                { "543d4676", "Raffay" },
                { "71e66ab3", "Aleena" },
                { "d13043b3", "Taylor" },
                { "de0e9b2c", "Anitta" }
            };

            var files = Directory.GetFiles(basePath, "*.csv");
            var grouped = new Dictionary<string, List<string>>();

            foreach (var f in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(f);
                var split = fileName.Split('_');
                if (split.Length != 2) continue;

                string id = split[0];
                string date = split[1];

                if (!grouped.ContainsKey(id))
                    grouped[id] = new List<string>();

                grouped[id].Add(date);
            }

            var list = grouped.Select(p => new CSVPatientViewModel
            {
                PatientId = p.Key,
                PatientName = nameMap.ContainsKey(p.Key) ? nameMap[p.Key] : "Unknown",
                ReadingCount = p.Value.Count,
                LatestDate = p.Value.OrderByDescending(x => x).First()
            }).ToList();

            return View(list);
        }

        // -------------------------
        // CLINICIANS
        // -------------------------
        public async Task<IActionResult> Clinicians()
        {
            var clinicians = await _userManager.GetUsersInRoleAsync("Clinical");

            var list = clinicians.Select(c => new ClinicianViewModel
            {
                Email = c.Email,
                CreatedAt = null,
                Source = "Internal"
            }).ToList();

            return View(list);
        }

        // -------------------------
        // SYSTEM SETTINGS
        // -------------------------
        [HttpGet]
        public IActionResult SystemSettings()
        {
            var settings = _context.SystemSettings.FirstOrDefault();

            if (settings == null)
            {
                settings = new SystemSetting
                {
                    Theme = "light",
                    EmailAlerts = false,
                    AnomalyAlerts = false,
                    RefreshRate = "10",
                    Timezone = "UTC"
                };

                _context.SystemSettings.Add(settings);
                _context.SaveChanges();
            }

            return View(settings);
        }

        [HttpPost]
        public IActionResult SystemSettings(SystemSetting model)
        {
            var settings = _context.SystemSettings.FirstOrDefault();

            if (settings != null)
            {
                settings.Theme = model.Theme;
                settings.EmailAlerts = model.EmailAlerts;
                settings.AnomalyAlerts = model.AnomalyAlerts;
                settings.RefreshRate = model.RefreshRate;
                settings.Timezone = model.Timezone;

                _context.SaveChanges();
                TempData["success"] = "Settings updated!";
            }

            return RedirectToAction("SystemSettings");
        }
    }
}
