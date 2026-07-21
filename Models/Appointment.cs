using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        
        [ForeignKey("PatientId")]
        public PatientProfile? Patient { get; set; }

        public int DoctorId { get; set; }
        
        [ForeignKey("DoctorId")]
        public DoctorProfile? Doctor { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public string? Notes { get; set; }
    }
}