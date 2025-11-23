using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;
using Software_Engineering_Final_Project_Team_Primal_Animals.ViewModels;
using Software_Engineering_Final_Project_Team_Primal_Animals.InputModels;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================================================
        //  PATIENT DASHBOARD
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Get logged-in ApplicationUser (Identity)
            string identityId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var loggedInUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == identityId);

            if (loggedInUser == null)
                return Unauthorized("User not found.");

            if (loggedInUser.AppUserId == null)
                return NotFound("This login account is not linked to an AppUser record.");

            int appUserId = loggedInUser.AppUserId.Value;

            // Load the Patient connected to this AppUser
            var patient = await _context.Patients
                .Include(p => p.SensorData)
                .FirstOrDefaultAsync(p => p.AppUserId == appUserId);

            if (patient == null)
                return NotFound("No patient profile found for this user.");

            // Latest sensor data frame
            var latestFrame = patient.SensorData
                .OrderByDescending(d => d.TimeStamp)
                .FirstOrDefault();
            if (latestFrame == null)
            {
                // ✅ Generate realistic, varied heatmap values (0–255)
                var random = new Random();
                latestFrame = new SensorData
                {
                    Pressure_Matrix = string.Join(",", Enumerable.Range(0, 1024)
                                                                 .Select(i => random.Next(0, 255))),
                    PeakPressureIndex = 230,    // High pressure for alert testing
                    Contact_Area = "47%",
                    TimeStamp = DateTime.Now
                };
            }



            // Build View Model
            var threshold = 180;
            bool highRisk = latestFrame.PeakPressureIndex >= threshold;

            var vm = new PatientDashboardVM
            {
                PressureMatrix = latestFrame.Pressure_Matrix,
                PeakPressure = latestFrame.PeakPressureIndex,
                ContactArea = latestFrame.Contact_Area,
                EmergencyName = patient.Emergency_contactName,
                EmergencyNumber = patient.Emergency_ContactNumber,
                Timestamp = latestFrame.TimeStamp,
                IsHighRisk = highRisk,
                AlertMessage = highRisk
                    ? "⚠ High pressure detected! Please reposition immediately."
                    : "✅ Pressure levels are safe.",
                EscalatedAlert = false
            };

            return View(vm);

        }

        // ================================================================
        //  POST COMMENT ON A SENSOR FRAME
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostComment(CommentInput input)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Dashboard));

            string identityId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var loggedInUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == identityId);

            if (loggedInUser == null)
                return Unauthorized();

            // Validate target frame
            var frame = await _context.SensorData
                .FirstOrDefaultAsync(d => d.Data_Id == input.Data_ID);

            if (frame == null)
                return RedirectToAction(nameof(Dashboard));

            // Create comment entry
            var comment = new CommentThread
            {
                User_IdentityId = identityId,   // identity user who made the comment
                Data_ID = input.Data_ID,
                Content = input.CommentText,
                Comment_Time = DateTime.Now
            };

            _context.CommentThreads.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }
    }
}

