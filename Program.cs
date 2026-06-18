using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "StudentCookie";
})
.AddCookie("StudentCookie", options =>
{
    options.LoginPath      = "/Student/Login";
    options.LogoutPath     = "/Student/Logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.Cookie.Name    = "Vcube.Student";
})
.AddCookie("StaffCookie", options =>
{
    options.LoginPath      = "/Staff/Login";
    options.LogoutPath     = "/Staff/Logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.Cookie.Name    = "Vcube.Staff";
});

var app = builder.Build();

// ── Auto-create DB on startup ──────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ── Middleware ──────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
