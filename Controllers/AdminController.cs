using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalSystem.Models;

namespace HospitalSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalDepartments = await _context.Departments.CountAsync();
            ViewBag.TotalDoctors = await _context.Users.CountAsync(u => u.Role == UserRole.Doctor);
            ViewBag.TotalPatients = await _context.Users.CountAsync(u => u.Role == UserRole.Patient);
            ViewBag.TotalAppointments = await _context.Appointments.CountAsync();
            
            return View();
        }

        // --- Departments ---
        public async Task<IActionResult> Departments()
        {
            var depts = await _context.Departments.ToListAsync();
            return View(depts);
        }

        [HttpGet]
        public IActionResult CreateDepartment()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Departments));
            }
            return View(department);
        }

        // --- Specializations ---
        public async Task<IActionResult> Specializations()
        {
            var specs = await _context.Specializations.Include(s => s.Department).ToListAsync();
            return View(specs);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSpecialization()
        {
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSpecialization(Specialization specialization)
        {
            if (ModelState.IsValid)
            {
                _context.Specializations.Add(specialization);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Specializations));
            }
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(specialization);
        }
        
        // --- Doctors Management ---
        public async Task<IActionResult> Doctors()
        {
            var doctors = await _context.Users
                .Include(u => u.DoctorProfile)
                .ThenInclude(dp => dp!.Specialization)
                .Where(u => u.Role == UserRole.Doctor)
                .ToListAsync();
            return View(doctors);
        }
        
        [HttpGet]
        public async Task<IActionResult> AssignSpecialization(int id)
        {
            var user = await _context.Users.Include(u => u.DoctorProfile).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null || user.Role != UserRole.Doctor) return NotFound();

            ViewBag.Specializations = await _context.Specializations.ToListAsync();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> AssignSpecialization(int id, int specializationId, decimal consultationFee, string bio)
        {
            var user = await _context.Users.Include(u => u.DoctorProfile).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null || user.Role != UserRole.Doctor) return NotFound();

            if (user.DoctorProfile == null)
            {
                user.DoctorProfile = new DoctorProfile
                {
                    UserId = user.Id,
                    SpecializationId = specializationId,
                    ConsultationFee = consultationFee,
                    Bio = bio
                };
                _context.DoctorProfiles.Add(user.DoctorProfile);
            }
            else
            {
                user.DoctorProfile.SpecializationId = specializationId;
                user.DoctorProfile.ConsultationFee = consultationFee;
                user.DoctorProfile.Bio = bio;
                _context.DoctorProfiles.Update(user.DoctorProfile);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Doctors));
        }

        // --- Patients Management ---
        public async Task<IActionResult> Patients()
        {
            var patients = await _context.Users
                .Include(u => u.PatientProfile)
                .Where(u => u.Role == UserRole.Patient)
                .ToListAsync();
            return View(patients);
        }

        // --- All Appointments ---
        public async Task<IActionResult> AllAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.Specialization)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
            return View(appointments);
        }

        // --- Update Credentials ---
        [HttpPost]
        public async Task<IActionResult> UpdateCredentials(int id, string username, string password, string returnUrl)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Check if username is already taken by another user
                if (await _context.Users.AnyAsync(u => u.Username == username && u.Id != id))
                {
                    TempData["ErrorMessage"] = "Username already exists. Please choose another.";
                    return Redirect(returnUrl ?? "/Admin");
                }

                user.Username = username;
                if (!string.IsNullOrEmpty(password))
                {
                    user.PasswordHash = password; // Plaintext for prototype
                }
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Credentials updated successfully for {user.FullName}.";
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
            }

            return Redirect(returnUrl ?? "/Admin");
        }
    }
}