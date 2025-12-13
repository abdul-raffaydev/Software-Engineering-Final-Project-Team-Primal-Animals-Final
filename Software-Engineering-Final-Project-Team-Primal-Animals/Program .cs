using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;
using System.Globalization;
using QuestPDF.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// For quest pdf
QuestPDF.Settings.License = LicenseType.Community;


// MVC
builder.Services.AddControllersWithViews();

// From In-Memory DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDB"));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// INITIAL DATA SEEDING
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    // ROLES
    string[] roles = { "Admin", "Clinical", "Patient" };
    foreach (var r in roles)
    {
        if (!await roleManager.RoleExistsAsync(r))
            await roleManager.CreateAsync(new IdentityRole(r));
    }

    // ADMIN USER
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

        var res = await userManager.CreateAsync(adminUser, "Admin123!");
        if (res.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }

     
    // CLINICIANS
    var clinicianSeed = new List<string>
    {
        "claire.wilson@test.com",
        "dr.jameson@test.com",
        "nurse.emma@test.com"
    };

    foreach (var email in clinicianSeed)
    {
        var u = await userManager.FindByEmailAsync(email);
        if (u == null)
        {
            u = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Full_Name = email.Split('@')[0].Replace(".", " ").ToUpper()
            };

            if ((await userManager.CreateAsync(u, "Clinician123!")).Succeeded)
                await userManager.AddToRoleAsync(u, "Clinical");
        }
    }

    // PATIENTS
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

    var heatmapFolder = Path.Combine(env.ContentRootPath, "App_Data", "Heatmaps");

    if (Directory.Exists(heatmapFolder))
    {
        var groups = Directory.GetFiles(heatmapFolder, "*.csv")
            .GroupBy(f => Path.GetFileNameWithoutExtension(f).Split('_')[0])
            .Take(5) 
            .ToList();

        int index = 0;

        foreach (var group in groups)
        {
            string name = patientNames[index];
            string emailName = name.ToLower().Replace(" ", "");
            string email = $"{emailName}@test.com";

            string age = patientAges[index];
            string dob = patientDob[index];

            index++;

            // CREATE USER
            var identityUser = await userManager.FindByEmailAsync(email);
            if (identityUser == null)
            {
                identityUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Full_Name = name
                };

                var result = await userManager.CreateAsync(identityUser, "Password123!");
                if (!result.Succeeded)
                    continue;

                await userManager.AddToRoleAsync(identityUser, "Patient");
            }

            // PATIENT PROFILE
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

            // Import CSV heatmap frames
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

                db.SensorData.Add(new SensorData
                {
                    Patient_ID = patient.Patient_ID,
                    TimeStamp = timestamp,
                    Pressure_Matrix = string.Join(",", allValues),
                    PeakPressureIndex = allValues.Max(),
                    Contact_Area = $"{(allValues.Count(v => v > 30) / (double)allValues.Count * 100.0):F1}%"
                });
            }

            await db.SaveChangesAsync();
        }
    }
 
    // SYSTEM SETTINGS
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
