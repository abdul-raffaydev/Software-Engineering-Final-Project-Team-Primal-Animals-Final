using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;
using System.Globalization;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

// MVC
builder.Services.AddControllersWithViews();

// In-Memory DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDB"));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


// ======================================================
// 🔁 AUTO-REDIRECT CLINICIAN AFTER LOGIN
// ======================================================
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true &&
        context.User.IsInRole("clinician") &&
        context.Request.Path == "/")
    {
        context.Response.Redirect("/Clinician/ClinicianDashboard");
        return;
    }

    await next();
});


// ======================================================
// INITIAL DATA SEEDING
// ======================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    // ---------------- ROLES ----------------
    string[] roles = { "Admin", "clinician", "Patient" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // ---------------- ADMIN ----------------
    var adminEmail = "admin@test.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            Full_Name = "System Admin"
        };

        if ((await userManager.CreateAsync(adminUser, "Admin123!")).Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // ---------------- CLINICIANS ----------------
    var clinicianSeed = new List<string>
    {
        "claire.wilson@test.com",
        "dr.jameson@test.com",
        "nurse.emma@test.com",
        "abdullah@test.com"     // ✅ NEW CLINICIAN
    };

    foreach (var email in clinicianSeed)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Full_Name = email.Split('@')[0].Replace(".", " ").ToUpper()
            };

            if ((await userManager.CreateAsync(user, "Password123!")).Succeeded)
                await userManager.AddToRoleAsync(user, "clinician");
        }
    }

    // ---------------- PATIENTS ----------------
    var patientNames = new[] { "Raffay", "Hassan", "Anitta", "Taylor", "Aleena" };
    var patientAges = new[] { "24", "27", "31", "40", "35" };
    var patientDob = new[] { "2000-01-01", "1997-03-15", "1993-05-20", "1984-09-10", "1989-11-25" };

    var heatmapFolder = Path.Combine(env.ContentRootPath, "App_Data", "Heatmaps");

    if (Directory.Exists(heatmapFolder))
    {
        var groups = Directory.GetFiles(heatmapFolder, "*.csv")
            .GroupBy(f => Path.GetFileNameWithoutExtension(f).Split('_')[0])
            .Take(5)
            .ToList();

        for (int i = 0; i < groups.Count; i++)
        {
            string name = patientNames[i];
            string email = $"{name.ToLower()}@test.com";

            var identityUser = await userManager.FindByEmailAsync(email);
            if (identityUser == null)
            {
                identityUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Full_Name = name
                };

                if (!(await userManager.CreateAsync(identityUser, "Password123!")).Succeeded)
                    continue;

                await userManager.AddToRoleAsync(identityUser, "Patient");
            }

            var patient = new Patient
            {
                Full_Name = name,
                Emergency_contactName = "Emergency Contact",
                Emergency_ContactNumber = 999999999,
                Age = patientAges[i],
                DateOfBirth = patientDob[i],
                AppUserId = identityUser.Id,
                HighPressureThreshold = 180
            };

            db.Patients.Add(patient);
            await db.SaveChangesAsync();

            foreach (var file in groups[i])
            {
                var parts = Path.GetFileNameWithoutExtension(file).Split('_');
                DateTime timestamp = DateTime.Now;

                if (parts.Length == 2 &&
                    DateTime.TryParseExact(parts[1], "yyyyMMdd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsed))
                {
                    timestamp = parsed;
                }

                var values = File.ReadAllLines(file)
                    .SelectMany(l => l.Split(',', ';', ' ', '\t'))
                    .Select(v => int.TryParse(v, out var n) ? n : -1)
                    .Where(v => v >= 0)
                    .ToList();

                if (!values.Any()) continue;

                db.SensorData.Add(new SensorData
                {
                    Patient_ID = patient.Patient_ID,
                    TimeStamp = timestamp,
                    Pressure_Matrix = string.Join(",", values),
                    PeakPressureIndex = values.Max(),
                    Contact_Area = $"{(values.Count(v => v > 30) / (double)values.Count * 100):F1}%"
                });
            }

            await db.SaveChangesAsync();
        }
    }

    // ---------------- SYSTEM SETTINGS ----------------
    if (!db.SystemSettings.Any())
    {
        db.SystemSettings.Add(new SystemSetting
        {
            Theme = "light",
            EmailAlerts = false,
            AnomalyAlerts = false,
            RefreshRate = "10",
            Timezone = "UTC"
        });

        await db.SaveChangesAsync();
    }
}

// ROUTING
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
