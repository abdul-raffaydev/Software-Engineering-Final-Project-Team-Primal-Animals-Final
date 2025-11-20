using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;
using Software_Engineering_Final_Project_Team_Primal_Animals.ViewModels;
using Software_Engineering_Final_Project_Team_Primal_Animals.InputModels;


namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Logged-in user's identity ID
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var patient = await _context.Patients
                .Include(p => p.SensorData)
                .FirstOrDefaultAsync(p => p.User_ID == userId);

            if (patient == null)
                return NotFound();

            var latestFrame = patient.SensorData
                .OrderByDescending(d => d.TimeStamp)
                .FirstOrDefault();

            if (latestFrame == null)
                return View(new PatientDashboardVM());

            var vm = new PatientDashboardVM
            {
                PressureMatrix = latestFrame.Pressure_Matrix,
                PeakPressure = latestFrame.PeakPressureIndex,
                ContactArea = latestFrame.Contact_Area,
                EmergencyName = patient.Emergency_contactName,
                EmergencyNumber = patient.Emergency_ContactNumber,
                Timestamp = latestFrame.TimeStamp
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> PostComment(CommentInput input)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var comment = new CommentThread
            {
                User_ID = userId,
                Data_ID = input.Data_ID,
                Content = input.CommentText,
                Comment_Time = DateTime.Now
            };

            _context.CommentThreads.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
    }
}
