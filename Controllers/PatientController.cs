using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HospitalSystem.Models;
using HospitalSystem.ViewModels;

namespace HospitalSystem.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var patientProfile = await _context.PatientProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patientProfile == null) return NotFound("Patient profile not found.");

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d!.User)
                .Include(a => a.Doctor)
                .ThenInclude(d => d!.Specialization)
                .Where(a => a.PatientId == patientProfile.Id)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Book()
        {
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(new BookAppointmentViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Book(BookAppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var patientProfile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                
                if (patientProfile != null)
                {
                    var appointment = new Appointment
                    {
                        PatientId = patientProfile.Id,
                        DoctorId = model.DoctorId, // Note: model.DoctorId is the DoctorProfile Id
                        AppointmentDate = model.AppointmentDate,
                        Notes = model.Notes,
                        Status = AppointmentStatus.Pending
                    };
                    
                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Appointment booked successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(model);
        }
    }
}