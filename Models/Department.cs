using System.ComponentModel.DataAnnotations;

namespace HospitalSystem.Models
{
    public class Department
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string? ImageUrl { get; set; }

        public List<Specialization> Specializations { get; set; } = new();
    }
}