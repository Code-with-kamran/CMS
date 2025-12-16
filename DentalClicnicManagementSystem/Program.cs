using CMS.Controllers;
using CMS.Data;

using CMS.Helpers;
using CMS.Hubs;
using CMS.Models;
using CMS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Bind Email settings from configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// Register custom email sender
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();

builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Logging.AddConsole();

builder.Services.AddScoped<FileHelper>();
builder.Services.AddScoped<SlotBuilder>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/Login";
            });

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPublicForms", policy =>
    {
        policy.AllowAnyOrigin()  // Or specify specific domains: .WithOrigins("https://example.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllersWithViews(o =>
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddRazorPages();

builder.Services.AddSession();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.SeedAdminUser(context);  // Seed Admin User
}
app.MapHub<SettingsHub>("/settingsHub");

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//app.UseSession();

// 🔹 Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "doctorDashboard",
    pattern: "DoctorDashboard/{id:int}",
    defaults: new { controller = "DoctorDashboard", action = "Index" }
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PublicAppointments}/{action=Index}/{id?}");

app.Run();



