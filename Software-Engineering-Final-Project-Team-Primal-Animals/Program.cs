using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Software_Engineering_Final_Project_Team_Primal_Animals.Data;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// InMemory Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDB"));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Middleware pipeline
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ========================================================
// AUTO-SEED ROLES, USERS, PATIENT, SENSOR DATA
// ========================================================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    string[] roles = { "Admin", "Clinical", "Patient" };

    // 1. CREATE ROLES
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // 2. CREATE ADMIN USER
    var adminUser = await userManager.FindByEmailAsync("admin@test.com");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin@test.com",
            Email = "admin@test.com"
        };

        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // 3. CLINICAL USER
    var clinicalUser = await userManager.FindByEmailAsync("clinical@test.com");
    if (clinicalUser == null)
    {
        clinicalUser = new ApplicationUser
        {
            UserName = "clinical@test.com",
            Email = "clinical@test.com"
        };

        await userManager.CreateAsync(clinicalUser, "Clinical123!");
        await userManager.AddToRoleAsync(clinicalUser, "Clinical");
    }

    // 4. PATIENT USER + APPUSER + PATIENT RECORD + HEATMAP DATA
    var patientUser = await userManager.FindByEmailAsync("patient@test.com");
    if (patientUser == null)
    {
        patientUser = new ApplicationUser
        {
            UserName = "patient@test.com",
            Email = "patient@test.com"
        };

        await userManager.CreateAsync(patientUser, "Patient123!");
        await userManager.AddToRoleAsync(patientUser, "Patient");

        // Create AppUser
        var appUserEntity = new AppUser
        {
            Full_Name = "John Doe",
            User_Email = "patient@test.com",
            Password_Hash = "N/A",
            Account_Status = "Active"
        };
        db.AppUsers.Add(appUserEntity);
        await db.SaveChangesAsync();

        // Link Identity → AppUser
        patientUser.AppUserId = appUserEntity.AppUserId;
        await userManager.UpdateAsync(patientUser);

        // Create Patient profile
        var patientEntity = new Patient
        {
            AppUserId = appUserEntity.AppUserId,
            Full_Name = "John Doe",
            Age = "32",
            DateOfBirth = "1992-05-21",
            Emergency_contactName = "Jane Doe",
            Emergency_ContactNumber = 911
        };
        db.Patients.Add(patientEntity);
        await db.SaveChangesAsync();

        // Create REAL sample heatmap data
        var random = new Random();
        var matrixValues = Enumerable.Range(0, 1024)
                                     .Select(x => random.Next(20, 240)); // realistic pressures

        db.SensorData.Add(new SensorData
        {
            Patient_ID = patientEntity.Patient_ID,
            TimeStamp = DateTime.Now,
            Pressure_Matrix = string.Join(",", matrixValues),
            PeakPressureIndex = 188,
            Contact_Area = "32%"
        });

        await db.SaveChangesAsync();
    }
}

// DEFAULT ROUTE
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


