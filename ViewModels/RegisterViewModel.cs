using System.ComponentModel.DataAnnotations;
using HospitalSystem.Models;

namespace HospitalSystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}