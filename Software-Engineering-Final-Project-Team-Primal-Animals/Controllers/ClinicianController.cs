using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;
using Software_Engineering_Final_Project_Team_Primal_Animals.ViewModels;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
    //[Authorize]
    public class ClinicianController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClinicianController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. CLINICIAN DASHBOARD
        // ============================================================
        public async Task<IActionResult> ClinicianDashboard(string search)
        {
            var query = _context.Patients
                .Include(p => p.SensorData)
                .Include(p => p.Comments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(p =>
                    p.Full_Name.ToLower().Contains(search) ||
                    p.Patient_ID.ToString().Contains(search));
            }

            var patients = await query.ToListAsync();

            var vm = patients.Select(p =>
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

            return View(vm);
        }

        // ============================================================
        // 2. PATIENT DETAILS PAGE
        // ============================================================
        public async Task<IActionResult> PatientDetails(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.SensorData)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Patient_ID == id);

            if (patient == null) return NotFound();

            var frames = patient.SensorData
                .OrderBy(s => s.TimeStamp)
                .Select(s => new FrameInfo
                {
                    DataId = s.Data_Id,
                    Timestamp = s.TimeStamp
                })
                .ToList();

            var latest = patient.SensorData
                .OrderByDescending(s => s.TimeStamp)
                .FirstOrDefault();

            // ------------------------------
            // BUILD HEATMAP FOR LATEST FRAME
            // ------------------------------
            List<List<int>> matrix = new();
            int matrixSize = 32;

            if (latest != null && !string.IsNullOrEmpty(latest.Pressure_Matrix))
            {
                var numbers = latest.Pressure_Matrix.Split(',')
                    .Select(x => int.TryParse(x, out var n) ? n : 0)
                    .ToList();

                int nsize = (int)Math.Sqrt(numbers.Count);
                if (nsize * nsize != numbers.Count)
                    nsize = 32;

                matrixSize = nsize;

                for (int r = 0; r < nsize; r++)
                {
                    var row = new List<int>();
                    for (int c = 0; c < nsize; c++)
                        row.Add(numbers[r * nsize + c]);
                    matrix.Add(row);
                }
            }

            // ------------------------------
            // TIME SERIES
            // ------------------------------
            var peakSeries = patient.SensorData.OrderBy(s => s.TimeStamp)
                .Select(s => new TimeSeriesPoint
                {
                    Time = s.TimeStamp,
                    Value = s.PeakPressureIndex
                })
                .ToList();

            var contactSeries = patient.SensorData.OrderBy(s => s.TimeStamp)
                .Select(s => new TimeSeriesPoint
                {
                    Time = s.TimeStamp,
                    Value = double.TryParse(s.Contact_Area.Replace("%", ""), out var d) ? d : 0
                })
                .ToList();

            // ------------------------------
            // COMMENTS
            // ------------------------------
            var comments = patient.Comments
                .OrderByDescending(c => c.Comment_Time)
                .Select(c => new CommentVM
                {
                    Author = "Clinician",
                    Content = c.Content,
                    CommentTime = c.Comment_Time
                })
                .ToList();

            var vm = new PatientDetailsVM
            {
                PatientId = patient.Patient_ID,
                PatientName = patient.Full_Name,
                Frames = frames,
                Comments = comments,
                PeakPressureIndex = latest?.PeakPressureIndex ?? 0,
                ContactArea = latest?.Contact_Area ?? "0%",
                HighPressureThreshold = patient.HighPressureThreshold,
                HeatmapJson = JsonSerializer.Serialize(matrix),
                MatrixSize = matrixSize,
                PeakPressureSeries = peakSeries,
                ContactAreaSeries = contactSeries
            };

            return View(vm);
        }

        // ============================================================
        // 3. API → RETURN SINGLE FRAME FOR HEATMAP
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetPatientFrame(int patientId, int dataId)
        {
            var frame = await _context.SensorData
                .FirstOrDefaultAsync(s => s.Patient_ID == patientId && s.Data_Id == dataId);

            if (frame == null) return NotFound();

            var numbers = frame.Pressure_Matrix.Split(',')
                .Select(x => int.TryParse(x, out var n) ? n : 0)
                .ToList();

            int size = (int)Math.Sqrt(numbers.Count);
            if (size * size != numbers.Count)
                size = 32;

            List<List<int>> matrix = new();

            for (int r = 0; r < size; r++)
            {
                var row = new List<int>();
                for (int c = 0; c < size; c++)
                    row.Add(numbers[r * size + c]);
                row.TrimExcess();
                matrix.Add(row);
            }

            return Json(new
            {
                DataId = frame.Data_Id,
                Timestamp = frame.TimeStamp,
                Matrix = matrix
            });
        }

        // ============================================================
        // 4. API → RETURN ALL FRAMES
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetPatientFrames(int patientId)
        {
            var frames = await _context.SensorData
                .Where(s => s.Patient_ID == patientId)
                .OrderBy(s => s.TimeStamp)
                .Select(s => new FrameInfo
                {
                    DataId = s.Data_Id,
                    Timestamp = s.TimeStamp
                })
                .ToListAsync();

            return Json(frames);
        }

        // ============================================================
        // 5. API → RETURN TIME SERIES FOR CHART
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetPatientTimeSeries(int patientId, string period)
        {
            DateTime cutoff = period switch
            {
                "1h" => DateTime.Now.AddHours(-1),
                "6h" => DateTime.Now.AddHours(-6),
                "24h" => DateTime.Now.AddDays(-1),
                "7d" => DateTime.Now.AddDays(-7),
                _ => DateTime.Now.AddDays(-7)
            };

            var data = await _context.SensorData
                .Where(s => s.Patient_ID == patientId && s.TimeStamp >= cutoff)
                .OrderBy(s => s.TimeStamp)
                .ToListAsync();

            var series = data.Select(x => new
            {
                time = x.TimeStamp,
                peak = x.PeakPressureIndex,
                contact = double.TryParse(x.Contact_Area.Replace("%", ""), out var d) ? d : 0
            });

            return Json(series);
        }

        // ============================================================
        // 6. SAVE COMMENT
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostComment(int patientId, int dataId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("Content required");

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Patient_ID == patientId);
            if (patient == null) return NotFound("Patient not found");

            var frame = await _context.SensorData
                .FirstOrDefaultAsync(s => s.Patient_ID == patientId && s.Data_Id == dataId);

            if (frame == null) return NotFound("Data frame not found");

            var comment = new CommentThread
            {
                Patient_ID = patientId,
                Data_ID = dataId,
                Content = content,
                Comment_Time = DateTime.UtcNow
            };

            _context.CommentThreads.Add(comment);
            await _context.SaveChangesAsync();

            return Json(new
            {
                Content = comment.Content,
                CommentTime = comment.Comment_Time,
                DataId = comment.Data_ID,
                PatientId = comment.Patient_ID
            });
        }

        // ============================================================
        // 7. UPDATE THRESHOLD
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateThreshold(int patientId, int threshold)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Patient_ID == patientId);
            if (patient == null) return NotFound();

            patient.HighPressureThreshold = threshold;
            await _context.SaveChangesAsync();

            return Json(new
            {
                HighPressureThreshold = threshold
            });
        }

        // ============================================================
        // 8. FIXED: GENERATE PDF REPORT
        // ============================================================
        [HttpGet]
        public IActionResult DownloadPatientReport(int patientId)
        {
            var patient = _context.Patients
                .FirstOrDefault(p => p.Patient_ID == patientId);

            if (patient == null)
                return NotFound("Patient not found.");

            int threshold = patient.HighPressureThreshold;

            // ✔ LOAD REAL SENSOR DATA HERE
            var data = _context.SensorData
                .Where(x => x.Patient_ID == patientId)
                .ToList();

            double avgPeak = data.Any()
                ? data.Average(x => x.PeakPressureIndex)
                : 0;

            double avgContact = data.Any()
                ? data.Average(x =>
                {
                    double v;
                    return double.TryParse(x.Contact_Area.Replace("%", ""), out v) ? v : 0;
                })
                : 0;

            // Build PDF
            byte[] pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Content().Column(col =>
                    {
                        col.Item().Text("Patient Report")
                            .FontSize(22).Bold();

                        col.Item().Text($"Patient Name: {patient.Full_Name}");
                        col.Item().Text($"Generated: {DateTime.Now}");
                        col.Item().LineHorizontal(1);

                        col.Item().Text($"Average Peak Pressure (24h): {avgPeak:F1}");
                        col.Item().Text($"Average Contact Area % (24h): {avgContact:F1}%");
                        col.Item().Text($"Pressure Threshold: {threshold}");

                        col.Item().LineHorizontal(1);
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"{patient.Full_Name}_Report.pdf");
        }

        // ============================================================
        // 9. GENERATE DATE-RANGE PDF
        // ============================================================
        [HttpGet]
        public IActionResult GeneratePdfReport(int patientId, DateTime from, DateTime to)
        {
            var patient = _context.Patients
                .FirstOrDefault(p => p.Patient_ID == patientId);

            if (patient == null)
                return NotFound();

            int threshold = patient.HighPressureThreshold;

            var data = _context.SensorData
                .Where(x => x.Patient_ID == patientId &&
                            x.TimeStamp >= from &&
                            x.TimeStamp <= to)
                .ToList();

            double avgPeak = data.Any()
                ? data.Average(x => x.PeakPressureIndex)
                : 0;

            double avgContact = data.Any()
                ? data.Average(x =>
                {
                    double v;
                    return double.TryParse(x.Contact_Area.Replace("%", ""), out v) ? v : 0;
                })
                : 0;

            byte[] pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Content().Column(col =>
                    {
                        col.Item().Text("Patient Report")
                            .FontSize(22).Bold();

                        col.Item().Text($"Name: {patient.Full_Name}");
                        col.Item().Text($"Date Range: {from:G} → {to:G}");
                        col.Item().LineHorizontal(1);

                        col.Item().Text($"Average Peak Pressure Index: {avgPeak:F1}");
                        col.Item().Text($"Average Contact Area %: {avgContact:F1}%");
                        col.Item().Text($"High Pressure Threshold: {threshold}");

                        col.Item().LineHorizontal(1);
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"{patient.Full_Name}_Report.pdf");
        }
    }
}
