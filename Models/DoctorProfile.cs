using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalSystem.Models
{
    public class DoctorProfile
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int SpecializationId { get; set; }
        
        [ForeignKey("SpecializationId")]
        public Specialization? Specialization { get; set; }

        public string? Bio { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ConsultationFee { get; set; }

        public List<Appointment> Appointments { get; set; } = new();
    }
}