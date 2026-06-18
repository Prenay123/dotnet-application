using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.Data;
using SchoolApp.Models;
using SchoolApp.ViewModels;
using System.Security.Claims;

namespace SchoolApp.Controllers
{
    public class StudentController : Controller
    {
        private readonly AppDbContext _db;
        private const string AuthScheme = "StudentCookie";

        public StudentController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Student/Register
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true && User.FindFirst("UserType")?.Value == "Student")
                return RedirectToAction("Dashboard");
            return View();
        }

        // POST: /Student/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(StudentRegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_db.Students.Any(s => s.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            var student = new Student
            {
                FullName     = model.FullName,
                Email        = model.Email,
                PhoneNumber  = model.PhoneNumber,
                Department   = model.Department,
                RollNumber   = model.RollNumber,
                EnrollmentYear = model.EnrollmentYear,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                CreatedAt    = DateTime.UtcNow
            };

            _db.Students.Add(student);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        // GET: /Student/Login
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true && User.FindFirst("UserType")?.Value == "Student")
                return RedirectToAction("Dashboard");
            return View();
        }

        // POST: /Student/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(StudentLoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var student = _db.Students.FirstOrDefault(s => s.Email == model.Email);
            if (student == null || !BCrypt.Net.BCrypt.Verify(model.Password, student.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()),
                new Claim(ClaimTypes.Name, student.FullName),
                new Claim(ClaimTypes.Email, student.Email),
                new Claim("UserType", "Student")
            };

            var identity  = new ClaimsIdentity(claims, AuthScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(AuthScheme, principal,
                new AuthenticationProperties { IsPersistent = true });

            return RedirectToAction("Dashboard");
        }

        // GET: /Student/Dashboard
        public IActionResult Dashboard()
        {
            if (User.Identity?.IsAuthenticated != true || User.FindFirst("UserType")?.Value != "Student")
                return RedirectToAction("Login");

            var id      = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var student = _db.Students.Find(id);
            if (student == null)
                return RedirectToAction("Login");

            return View(student);
        }

        // POST: /Student/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(AuthScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
