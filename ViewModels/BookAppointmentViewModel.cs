using System.ComponentModel.DataAnnotations;

namespace HospitalSystem.ViewModels
{
    public class BookAppointmentViewModel
    {
        [Required(ErrorMessage = "Please select a department.")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Please select a specialization.")]
        public int SpecializationId { get; set; }

        [Required(ErrorMessage = "Please select a doctor.")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Please select an appointment date.")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        public string? Notes { get; set; }
    }
}