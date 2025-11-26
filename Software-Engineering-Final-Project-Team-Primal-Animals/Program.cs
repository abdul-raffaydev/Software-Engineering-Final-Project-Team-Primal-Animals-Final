using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDB"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ===============================
// SEED PATIENTS + CSV HEATMAP DATA
// ===============================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    // Ensure Patient role exists
    if (!await roleManager.RoleExistsAsync("Patient"))
        await roleManager.CreateAsync(new IdentityRole("Patient"));

    // 5 patients
    var patientNames = new List<string>
    {
        "Raffay",
        "Hassan",
        "Anitta",
        "Taylor",
        "Aleena"
    };

    var patientAges = new List<string>
    {
        "24","27","31","40","35"
    };

    var patientDob = new List<string>
    {
        "2000-01-01","1997-03-15","1993-05-20","1984-09-10","1989-11-25"
       
    };

    // CSVs location
    var heatmapFolder = Path.Combine(env.ContentRootPath, "App_Data", "Heatmaps");

    if (Directory.Exists(heatmapFolder))
    {
        var groups = Directory.GetFiles(heatmapFolder, "*.csv")
            .GroupBy(f => Path.GetFileNameWithoutExtension(f).Split('_')[0])
            .ToList();

        int index = 0;

        foreach (var group in groups)
        {
            string name = index < patientNames.Count ? patientNames[index] : $"Patient {index + 1}";
            string emailName = name.ToLower().Replace(" ", "");
            string email = $"{emailName}@test.com";

            string age = index < patientAges.Count ? patientAges[index] : "Unknown";
            string dob = index < patientDob.Count ? patientDob[index] : "Unknown";

            index++;

            // Create Identity user (with full name)
            var identityUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Full_Name = name
            };

            var result = await userManager.CreateAsync(identityUser, "Password123!");
            if (!result.Succeeded)
            {
                // If user already exists or any issue, skip to next
                continue;
            }

            await userManager.AddToRoleAsync(identityUser, "Patient");

            // Create Patient profile
            var patient = new Patient
            {
                Full_Name = name,
                Emergency_contactName = "Emergency Contact",
                Emergency_ContactNumber = 999999999,
                Age = age,
                DateOfBirth = dob,
                AppUserId = identityUser.Id,
                HighPressureThreshold = 180
            };

            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            // Import CSV frames for this patient
            foreach (var file in group)
            {
                var filename = Path.GetFileNameWithoutExtension(file);
                var parts = filename.Split('_');
                DateTime timestamp = DateTime.Now;

                if (parts.Length == 2 &&
                    DateTime.TryParseExact(parts[1], "yyyyMMdd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsed))
                {
                    timestamp = parsed;
                }

                var lines = File.ReadAllLines(file);
                var allValues = lines
                    .SelectMany(line => line.Split(',', ';', ' ', '\t'))
                    .Select(v => int.TryParse(v.Trim(), out var num) ? num : -1)
                    .Where(v => v >= 0)
                    .ToList();

                if (allValues.Count == 0)
                    continue;

                string matrix = string.Join(",", allValues);
                int peak = allValues.Max();

                int threshold = 30;
                double contactPct = (allValues.Count(v => v > threshold) / (double)allValues.Count) * 100.0;

                db.SensorData.Add(new SensorData
                {
                    Patient_ID = patient.Patient_ID,
                    TimeStamp = timestamp,
                    Pressure_Matrix = matrix,
                    PeakPressureIndex = peak,
                    Contact_Area = $"{contactPct:F1}%"
                });
            }

            await db.SaveChangesAsync();
        }
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

