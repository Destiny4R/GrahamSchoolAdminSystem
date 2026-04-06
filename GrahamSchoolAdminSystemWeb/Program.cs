using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemAccess.ServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemWeb.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
builder.Services.AddRazorPages();

// Add HTTP context accessor for audit logging
builder.Services.AddHttpContextAccessor();

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(j => {
    j.UseMySql(connection, ServerVersion.AutoDetect(connection));
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(Options =>
{
    //Accont Lockout configuration
    Options.Lockout.MaxFailedAccessAttempts = 3;
    //Options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(35800);
    // SETTING UP YOUR CUSTOM PASSWORD REQUIREMENTS
    Options.Password.RequiredLength = 0;
    Options.Password.RequiredUniqueChars = 0;
    Options.Password.RequireNonAlphanumeric = false;
    Options.Password.RequireDigit = false;
    Options.Password.RequireLowercase = false;
    Options.Password.RequireUppercase = false;
    Options.User.AllowedUserNameCharacters = "/abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    //Email confirmation configuration
    //Options.SignIn.RequireConfirmedEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddScoped<IFinanceServices, FinanceServices>();
builder.Services.AddScoped<IUsersServices,  UsersServices>();
builder.Services.AddScoped<IStudentServices, StudentServices>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<ISystemActivitiesServices, SystemActivitiesServices>();
builder.Services.AddScoped<ISchoolClassServices, SchoolClassServices>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ITermRegistrationServices, TermRegistrationServices>();
builder.Services.AddScoped<IFeesPaymentServices, FeesPaymentServices>();
builder.Services.AddScoped<IPTAPaymentServices, PTAPaymentServices>();
builder.Services.AddScoped<IOtherPaymentServices, OtherPaymentServices>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register the audit logging filter
builder.Services.AddScoped<AuditLoggingFilter>();

builder.Services.ConfigureApplicationCookie(a =>
{
    a.LoginPath = $"/account/login";
    a.LogoutPath = $"/Account/Logout";
    a.AccessDeniedPath = $"/account/accessdenied";
    a.ExpireTimeSpan = TimeSpan.FromDays(1);
    a.SlidingExpiration = true;
});

var app = builder.Build();

// Initialize database with seed data (roles, positions, and admin user)
await DbInitializer.Initialize(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
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

