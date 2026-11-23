using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    // Спеціальність лікаря
    public class Specialty
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Це поле обов'язкове")]
        [StringLength(50, ErrorMessage = "Максимум 50 символів")]
        [Display(Name = "Назва спеціальності")]
        public string Name { get; set; } = "";

        // Лікарі цієї спеціальності
        public List<DoctorProfile> Doctors { get; set; } = new();
    }
}
