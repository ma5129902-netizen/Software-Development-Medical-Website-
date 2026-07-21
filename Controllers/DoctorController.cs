using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HospitalSystem.Models;
using HospitalSystem.ViewModels;

namespace HospitalSystem.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctorProfile == null)
            {
                return NotFound("Doctor profile not found. Please contact admin to set up your profile.");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p!.User)
                .Where(a => a.DoctorId == doctorProfile.Id)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, AppointmentStatus status)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctorProfile == null) return Unauthorized();

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctorProfile.Id);
            
            if (appointment != null)
            {
                appointment.Status = status;
                await _context.SaveChangesAsync();
                return Ok();
            }

            return NotFound();
        }

        // --- My Patients ---
        public async Task<IActionResult> MyPatients()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctorProfile == null) return NotFound();

            // Get unique patients that have an appointment with this doctor
            var patients = await _context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p!.User)
                .Where(a => a.DoctorId == doctorProfile.Id)
                .Select(a => a.Patient)
                .Distinct()
                .ToListAsync();

            return View(patients);
        }

        // --- Profile Management ---
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.Include(u => u.DoctorProfile).FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null || user.DoctorProfile == null) return NotFound();

            var model = new DoctorProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                ConsultationFee = user.DoctorProfile.ConsultationFee,
                Bio = user.DoctorProfile.Bio
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(DoctorProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var user = await _context.Users.Include(u => u.DoctorProfile).FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user != null && user.DoctorProfile != null)
                {
                    user.FullName = model.FullName;
                    user.Email = model.Email;
                    user.Phone = model.Phone;
                    user.DoctorProfile.ConsultationFee = model.ConsultationFee;
                    user.DoctorProfile.Bio = model.Bio;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Profile updated successfully.";
                    return RedirectToAction(nameof(Profile));
                }
            }
            return View(model);
        }
    }
}