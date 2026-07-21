using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalSystem.Models;

namespace HospitalSystem.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DataController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("specializations/{departmentId}")]
        public async Task<IActionResult> GetSpecializations(int departmentId)
        {
            var specializations = await _context.Specializations
                .Where(s => s.DepartmentId == departmentId)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();
            return Ok(specializations);
        }

        [HttpGet("doctors/{specializationId}")]
        public async Task<IActionResult> GetDoctors(int specializationId)
        {
            var doctors = await _context.DoctorProfiles
                .Include(dp => dp.User)
                .Where(dp => dp.SpecializationId == specializationId)
                .Select(dp => new { 
                    dp.Id, 
                    Name = dp.User!.FullName, 
                    Fee = dp.ConsultationFee 
                })
                .ToListAsync();
            return Ok(doctors);
        }
    }
}