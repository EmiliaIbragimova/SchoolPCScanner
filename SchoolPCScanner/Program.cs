using Microsoft.EntityFrameworkCore;
using SchoolPCScanner.Models;
using SchoolPCScanner.Services;
using Microsoft.AspNetCore.Identity;
using SchoolPCScanner.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
// Configure SMTP settings
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
// registreer elke service bij de DI container
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IDamageTypeService, DamageTypeService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IDamageRegistrationService, DamageRegistrationService>();
builder.Services.AddScoped<ITerminationRegistrationService, TerminationRegistrationService>();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SchoolPCScannerDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole>().AddEntityFrameworkStores<SchoolPCScannerDbContext>();

var app = builder.Build();

var serviceProvider = app.Services.CreateScope().ServiceProvider;
var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
await roleManager.CreateAsync(new IdentityRole("Administrator"));
await roleManager.CreateAsync(new IdentityRole("Reception"));

var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

var adminEmails = new List<string>
{
    "ibragimovaemilia917@gmail.com",
    "mala@piso.be",
    "mabr@piso.be",
    "nedo@piso.be",
    "sila@piso.be"
};

foreach (var email in adminEmails)
{
    var adminUser = await userManager.FindByEmailAsync(email);
    if (adminUser != null)
    {
        await userManager.AddToRoleAsync(adminUser, "Administrator");
    }
    else
    {
        // Optioneel: log of handel het geval af waar de gebruiker niet gevonden wordt
        Console.WriteLine($"Gebruiker met e-mailadres {email} niet gevonden.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


