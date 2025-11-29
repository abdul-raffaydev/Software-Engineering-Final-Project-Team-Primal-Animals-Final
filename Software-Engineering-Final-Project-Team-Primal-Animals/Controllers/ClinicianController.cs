using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Final_Project_Team_Primal_Animals.ViewModels;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models; // Added this for the Patient model

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
  
    public class ClinicianController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClinicianController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DASHBOARD
        public async Task<IActionResult> ClinicianDashboard(string search)
        {
            var patientsQuery = _context.Patients
                .Include(p => p.SensorData)
                .Include(p => p.Comments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                patientsQuery = patientsQuery.Where(p =>
                    p.Full_Name.ToLower().Contains(search) ||
                    p.Patient_ID.ToString().Contains(search)
                );
            }

            var patients = await patientsQuery.ToListAsync();

            var viewModel = patients.Select(p =>
            {
                var latestData = p.SensorData.OrderByDescending(s => s.TimeStamp).FirstOrDefault();
                var latestComment = p.Comments.OrderByDescending(c => c.Comment_Time).FirstOrDefault();
                string status, cue, lastActivity;

                if (latestData != null && latestData.PeakPressureIndex > p.HighPressureThreshold)
                {
                    status = "High Pressure Alert";
                    cue = "Critical spike detected";
                    lastActivity = $"Alert at {latestData.TimeStamp:t}";
                }
                else if (latestComment != null)
                {
                    status = "New Comment";
                    cue = latestComment.Content;
                    lastActivity = $"Comment at {latestComment.Comment_Time:t}";
                }
                else
                {
                    status = "OK";
                    cue = "No issues";
                    lastActivity = latestData != null
                        ? $"Data synced {(int)(DateTime.Now - latestData.TimeStamp).TotalMinutes}m ago"
                        : "No sensor data yet";
                }

                return new ClinicianDashboardVM
                {
                    PatientId = p.Patient_ID,
                    PatientName = p.Full_Name,
                    Status = status,
                    Cue = cue,
                    LastActivity = lastActivity
                };
            }).ToList();

            return View(viewModel);
        }

        // 2. THE MISSING PIECE: PATIENT DETAILS
        public async Task<IActionResult> PatientDetails(int id)
        {
            if (id == 0) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.SensorData)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Patient_ID == id);

            if (patient == null) return NotFound();

            return View(patient);
        }
    }
}