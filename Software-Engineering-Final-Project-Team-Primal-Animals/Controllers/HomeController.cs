using Microsoft.AspNetCore.Mvc;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
