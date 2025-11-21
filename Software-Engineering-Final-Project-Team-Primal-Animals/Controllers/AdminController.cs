using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            // later: manage users, assign roles, system overview
            return View();
        }
    }
}

