using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;
using Software_Engineering_Final_Project_Team_Primal_Animals.ViewModels;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        // =========================
        // LOGIN (GET)
        // =========================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // =========================
        // LOGIN (POST) ✅ FIXED
        // =========================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            // ✅ ROLE-BASED REDIRECTION (CORRECT & SAFE)

            if (await _userManager.IsInRoleAsync(user, "Clinician"))
                return RedirectToAction("ClinicianDashboard", "Clinician");

            if (await _userManager.IsInRoleAsync(user, "Patient"))
                return RedirectToAction("Dashboard", "Patient");

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("AdminDashboard", "Admin");

            // fallback
            return RedirectToAction("Index", "Home");
        }

        // =========================
        // LOGOUT
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // =========================
        // REGISTER
        // =========================
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Full_Name = $"{model.FirstName} {model.LastName}"
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Patient");

            var age = DateTime.Now.Year - model.DateOfBirth.Year;
            if (model.DateOfBirth.Date > DateTime.Now.AddYears(-age)) age--;

            var random = new Random();

            var patient = new Patient
            {
                Full_Name = user.Full_Name,
                DateOfBirth = model.DateOfBirth.ToString("yyyy-MM-dd"),
                Age = age.ToString(),
                Emergency_contactName = $"{model.FirstName} Emergency",
                Emergency_ContactNumber = random.Next(300000000, 999999999),
                AppUserId = user.Id,
                HighPressureThreshold = 180
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            await _signInManager.SignInAsync(user, false);
            return RedirectToAction("Dashboard", "Patient");
        }

        // =========================
        // PATIENT SETTINGS
        // =========================
        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.AppUserId == userId);

            if (patient == null)
                return NotFound();

            return View(new AccountSettingsViewModel
            {
                FullName = patient.Full_Name,
                DateOfBirth = patient.DateOfBirth,
                EmergencyContactName = patient.Emergency_contactName,
                EmergencyContactNumber = patient.Emergency_ContactNumber.ToString()
            });
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(AccountSettingsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.AppUserId == userId);

            if (patient == null)
                return NotFound();

            patient.Full_Name = model.FullName;
            patient.DateOfBirth = model.DateOfBirth;
            patient.Emergency_contactName = model.EmergencyContactName;

            if (int.TryParse(model.EmergencyContactNumber, out int num))
                patient.Emergency_ContactNumber = num;

            await _context.SaveChangesAsync();

            ViewBag.Message = "Account settings updated.";
            return View(model);
        }
    }
}
