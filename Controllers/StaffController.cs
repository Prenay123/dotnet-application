using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.Data;
using SchoolApp.Models;
using SchoolApp.ViewModels;
using System.Security.Claims;

namespace SchoolApp.Controllers
{
    public class StaffController : Controller
    {
        private readonly AppDbContext _db;
        private const string AuthScheme = "StaffCookie";

        public StaffController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Staff/Register
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true && User.FindFirst("UserType")?.Value == "Staff")
                return RedirectToAction("Dashboard");
            return View();
        }

        // POST: /Staff/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(StaffRegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_db.TeachingStaff.Any(s => s.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            var staff = new TeachingStaff
            {
                FullName      = model.FullName,
                Email         = model.Email,
                PhoneNumber   = model.PhoneNumber,
                Department    = model.Department,
                Designation   = model.Designation,
                EmployeeId    = model.EmployeeId,
                Qualification = model.Qualification,
                JoiningDate   = model.JoiningDate,
                PasswordHash  = BCrypt.Net.BCrypt.HashPassword(model.Password),
                CreatedAt     = DateTime.UtcNow
            };

            _db.TeachingStaff.Add(staff);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        // GET: /Staff/Login
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true && User.FindFirst("UserType")?.Value == "Staff")
                return RedirectToAction("Dashboard");
            return View();
        }

        // POST: /Staff/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(StaffLoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var staff = _db.TeachingStaff.FirstOrDefault(s => s.Email == model.Email);
            if (staff == null || !BCrypt.Net.BCrypt.Verify(model.Password, staff.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, staff.Id.ToString()),
                new Claim(ClaimTypes.Name, staff.FullName),
                new Claim(ClaimTypes.Email, staff.Email),
                new Claim("UserType", "Staff")
            };

            var identity  = new ClaimsIdentity(claims, AuthScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(AuthScheme, principal,
                new AuthenticationProperties { IsPersistent = true });

            return RedirectToAction("Dashboard");
        }

        // GET: /Staff/Dashboard
        public IActionResult Dashboard()
        {
            if (User.Identity?.IsAuthenticated != true || User.FindFirst("UserType")?.Value != "Staff")
                return RedirectToAction("Login");

            var id    = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var staff = _db.TeachingStaff.Find(id);
            if (staff == null)
                return RedirectToAction("Login");

            return View(staff);
        }

        // POST: /Staff/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(AuthScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
