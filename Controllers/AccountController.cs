using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HospitalSystem.Models;
using HospitalSystem.ViewModels;

namespace HospitalSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.PasswordHash == model.Password); // simple plain text password for prototype
                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role.ToString()),
                        new Claim("FullName", user.FullName)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    if (user.Role == UserRole.Admin) return RedirectToAction("Index", "Admin");
                    if (user.Role == UserRole.Doctor) return RedirectToAction("Index", "Doctor");
                    return RedirectToAction("Index", "Patient");
                }
                ModelState.AddModelError("", "Invalid username or password");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = model.Password, // keeping simple for prototype
                    FullName = model.FullName,
                    Role = model.Role,
                    Email = model.Email,
                    Phone = model.Phone
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (user.Role == UserRole.Doctor)
                {
                    // For Doctors, they need a profile. We will just add an empty one for now, or admin adds it later. 
                    // Usually admin creates doctors. But if self-register, we can do this.
                    // For simplicity, let's assume they must be assigned a Specialization by Admin.
                }
                else if (user.Role == UserRole.Patient)
                {
                    var patientProfile = new PatientProfile { UserId = user.Id };
                    _context.PatientProfiles.Add(patientProfile);
                    await _context.SaveChangesAsync();
                }

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim("FullName", user.FullName)
                }, CookieAuthenticationDefaults.AuthenticationScheme)));

                if (user.Role == UserRole.Admin) return RedirectToAction("Index", "Admin");
                if (user.Role == UserRole.Doctor) return RedirectToAction("Index", "Doctor");
                return RedirectToAction("Index", "Patient");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}