using Microsoft.AspNetCore.Mvc;

public class ClinicianController : Controller
{
    public IActionResult PatientDetails()
    {
        return View();
    }
}
