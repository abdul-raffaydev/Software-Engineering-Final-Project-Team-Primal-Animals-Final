using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;
using System.Threading.Tasks;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return View();

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

            if (!result.Succeeded)
                return View();

            // ROLE-BASED REDIRECT
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Dashboard", "Admin");

            if (await _userManager.IsInRoleAsync(user, "Clinical"))
                return RedirectToAction("Dashboard", "Clinical");

            if (await _userManager.IsInRoleAsync(user, "Patient"))
                return RedirectToAction("Dashboard", "Patient");

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword, string role)
        {
            if (password != confirmPassword)
                return View();

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return View();

            // Assign selected role
            await _userManager.AddToRoleAsync(user, role);

            // Redirect after registration
            return RedirectToAction("Login");
        }
    [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }

            // Generate reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Display the token to the user (development only)
            ViewBag.Token = token;
            ViewBag.UserId = user.Id;

            return View("ForgotPasswordConfirmation");
        }
        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            ViewBag.UserId = userId;
            ViewBag.Token = token;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string userId, string token, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
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
            return View();
        }

    }
}


