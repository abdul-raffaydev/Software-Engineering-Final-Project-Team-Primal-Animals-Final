using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
