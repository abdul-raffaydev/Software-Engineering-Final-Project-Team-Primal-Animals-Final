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

        // LOGIN
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
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

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Dashboard", "Patient");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        // LOGOUT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // REGISTER
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

            // 1) Create Identity user
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Full_Name = $"{model.FirstName} {model.LastName}"
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Ensure Patient role exists
                if (!await _userManager.IsInRoleAsync(user, "Patient"))
                {
                    var roleManager = HttpContext.RequestServices.GetService(typeof(RoleManager<IdentityRole>)) as RoleManager<IdentityRole>;
                    if (roleManager != null && !await roleManager.RoleExistsAsync("Patient"))
                    {
                        await roleManager.CreateAsync(new IdentityRole("Patient"));
                    }

                    await _userManager.AddToRoleAsync(user, "Patient");
                }

                // 2) Create linked Patient profile
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

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Dashboard", "Patient");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // FORGOT PASSWORD
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Please enter your email.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }

            // Generate reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // For development: show token directly
            ViewBag.Token = token;
            ViewBag.UserId = user.Id;

            return View("ForgotPasswordConfirmation");
        }

        // RESET PASSWORD
        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            ViewBag.UserId = userId;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string token, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                return View();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
                return RedirectToAction("Login");

            ViewBag.Error = "Invalid or expired token.";
            ViewBag.UserId = userId;
            ViewBag.Token = token;
            return View();
        }

        // ACCOUNT SETTINGS FOR PATIENTS
        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            string identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.AppUserId == identityUserId);

            if (patient == null)
                return NotFound("No patient profile found.");

            var vm = new AccountSettingsViewModel
            {
                FullName = patient.Full_Name,
                DateOfBirth = patient.DateOfBirth,
                EmergencyContactName = patient.Emergency_contactName,
                EmergencyContactNumber = patient.Emergency_ContactNumber.ToString()
            };

            return View(vm);
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(AccountSettingsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.AppUserId == identityUserId);

            if (patient == null)
                return NotFound("No patient profile found.");

            // Update patient data
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

