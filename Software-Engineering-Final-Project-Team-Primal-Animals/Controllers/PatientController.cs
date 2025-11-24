using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Final_Project_Team_Primal_Animals.ViewModels;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        // ✅ PATIENT DASHBOARD
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // ✅ Logged-in Identity User ID (GUID string)
            string identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ✅ Find the matching patient using AppUserId (string)
            var patient = await _context.Patients
                .Include(p => p.SensorData)
                .FirstOrDefaultAsync(p => p.AppUserId == identityUserId);

            if (patient == null)
                return Unauthorized("No patient profile linked to this login.");

            // ✅ Get latest heatmap frame
            var latestFrame = patient.SensorData
                .OrderByDescending(s => s.TimeStamp)
                .FirstOrDefault();

            if (latestFrame == null)
            {
                return View(new PatientDashboardVM
                {
                    PatientName = patient.Full_Name,
                    EmergencyName = patient.Emergency_contactName,
                    EmergencyNumber = patient.Emergency_ContactNumber
                });
            }

            // ✅ Trend Data (Peak pressure over time)
            var trendFrames = patient.SensorData
                .OrderBy(s => s.TimeStamp)
                .ToList();

            // ✅ High-risk threshold
            bool highRisk = latestFrame.PeakPressureIndex >= 180;

            // ✅ Build ViewModel
            var vm = new PatientDashboardVM
            {
                PatientName = patient.Full_Name,

                PressureMatrix = latestFrame.Pressure_Matrix,
                PeakPressure = latestFrame.PeakPressureIndex,
                ContactArea = latestFrame.Contact_Area,
                Timestamp = latestFrame.TimeStamp,
                DataId = latestFrame.Data_Id,

                EmergencyName = patient.Emergency_contactName,
                EmergencyNumber = patient.Emergency_ContactNumber,

                IsHighRisk = highRisk,
                AlertMessage = highRisk
                    ? "⚠ High pressure detected! Please reposition immediately."
                    : "✅ Pressure levels are safe.",

                TrendLabels = trendFrames.Select(f => f.TimeStamp.ToShortDateString()).ToList(),
                TrendValues = trendFrames.Select(f => f.PeakPressureIndex).ToList()
            };

            return View(vm);
        }

        // ================================================================
        // ✅ POST COMMENT ON A SENSOR FRAME
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostComment(int Data_ID, string CommentText)
        {
            if (string.IsNullOrWhiteSpace(CommentText))
                return RedirectToAction(nameof(Dashboard));

            string identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.AppUserId == identityUserId);

            if (patient == null)
                return Unauthorized();

            var frame = await _context.SensorData
                .FirstOrDefaultAsync(d => d.Data_Id == Data_ID);

            if (frame == null)
                return RedirectToAction(nameof(Dashboard));

            var comment = new CommentThread
            {
                Patient_ID = patient.Patient_ID,
                Data_ID = Data_ID,
                Content = CommentText,
                Comment_Time = DateTime.Now
            };

            _context.CommentThreads.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
