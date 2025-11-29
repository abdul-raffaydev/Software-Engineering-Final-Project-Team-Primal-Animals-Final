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
   // [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================
        // PATIENT DASHBOARD
        // ============================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            string identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await _context.Patients
                .Include(p => p.SensorData)
                .FirstOrDefaultAsync(p => p.AppUserId == identityUserId);

            if (patient == null)
                return Unauthorized("No patient profile linked to this login.");

            var latestFrame = patient.SensorData
                .OrderByDescending(s => s.TimeStamp)
                .FirstOrDefault();

            if (latestFrame == null)
            {
                return View(new PatientDashboardVM
                {
                    PatientName = patient.Full_Name,
                    EmergencyName = patient.Emergency_contactName,
                    EmergencyNumber = patient.Emergency_ContactNumber,
                    HighPressureThreshold = patient.HighPressureThreshold,
                    TrendLabels = new(),
                    TrendValues = new()
                });
            }

            // SAFETY: Trim/normalize matrix to 1024 values max (32x32)
            string trimmedMatrix = "";
            if (!string.IsNullOrWhiteSpace(latestFrame.Pressure_Matrix))
            {
                var vals = latestFrame.Pressure_Matrix
                    .Split(',', ';', ' ', '\t')
                    .Select(v => v.Trim())
                    .Where(v => int.TryParse(v, out _))
                    .Take(1024) // only first 1024 cells
                    .ToList();

                // Pad if fewer than 1024, so grid is full
                while (vals.Count < 1024)
                    vals.Add("0");

                trimmedMatrix = string.Join(",", vals);
            }

            var trendFrames = patient.SensorData
                .OrderBy(s => s.TimeStamp)
                .ToList();

            int threshold = patient.HighPressureThreshold > 0 ? patient.HighPressureThreshold : 180;
            bool highRisk = latestFrame.PeakPressureIndex >= threshold;

            var vm = new PatientDashboardVM
            {
                PatientName = patient.Full_Name,
                PatientId = patient.Patient_ID,
                PressureMatrix = trimmedMatrix,
                PeakPressure = latestFrame.PeakPressureIndex,
                ContactArea = latestFrame.Contact_Area,
                Timestamp = latestFrame.TimeStamp,
                DataId = latestFrame.Data_Id,

                EmergencyName = patient.Emergency_contactName,
                EmergencyNumber = patient.Emergency_ContactNumber,

                HighPressureThreshold = threshold,
                IsHighRisk = highRisk,
                AlertMessage = highRisk
                    ? $"⚠ High pressure detected (Peak {latestFrame.PeakPressureIndex})! Your threshold is {threshold}."
                    : $"✅ Pressure levels are below your threshold of {threshold}.",

                TrendLabels = trendFrames.Select(f => f.TimeStamp.ToShortDateString()).ToList(),
                TrendValues = trendFrames.Select(f => f.PeakPressureIndex).ToList()
            };

            return View(vm);
        }

        // ============================
        // UPDATE THRESHOLD
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateThreshold(int threshold)
        {
            string identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.AppUserId == identityUserId);

            if (patient == null)
                return Unauthorized();

            if (threshold < 80) threshold = 80;
            if (threshold > 255) threshold = 255;

            patient.HighPressureThreshold = threshold;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }

        // ============================
        // POST COMMENT
        // ============================
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

