using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
   [Authorize(Roles = "Clinical")]
    public class ClinicalController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}

