using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHeatMapService _heatmapService;

        public PatientController(ApplicationDbContext context, IHeatMapService heatmapService)
        {
            _context = context;
            _heatmapService = heatmapService;
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.Patients
                  .Include(x => x.PressureFrames)
                  .FirstOrDefaultAsync(x => x.UserId == userId);

            var latestFrame = patient.PressureFrames.OrderByDescending(x => x.Timestamp).First();

            var peakPressureIndex = _heatmapService.GetPeakPressureIndex(latestFrame);
            var contactAreaPercentage = _heatmapService.GetContactAreaPercentage(latestFrame);

            var vm = new PatientDashboardVM
            {
                HeatMap = latestFrame.FrameData,
                PeakPressure = peakPressureIndex,
                ContactArea = contactAreaPercentage,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactNumber = patient.EmergencyContactNumber
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> PostComment(CommentInput model)
        {
            // saves user comment associated to timestamp frame
            // this supports clinician thread reply later
        }
    }
}
}
