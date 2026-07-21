using System.ComponentModel.DataAnnotations;

namespace HospitalSystem.ViewModels
{
    public class DoctorProfileViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [Display(Name = "Consultation Fee ($)")]
        [Range(0, 10000, ErrorMessage = "Fee must be between 0 and 10000.")]
        public decimal ConsultationFee { get; set; }

        [Display(Name = "Professional Bio")]
        public string? Bio { get; set; }
    }
}