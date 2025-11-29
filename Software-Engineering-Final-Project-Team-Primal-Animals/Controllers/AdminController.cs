using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // ✅ Show all users
        public IActionResult Index()
        {
            var users = _userManager.Users;
            return View(users);
        }

        // ✅ Delete a user
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
                await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}

