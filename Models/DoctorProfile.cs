using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    // Профіль лікаря: прив'язаний до користувача і спеціальності
    public class DoctorProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        [Display(Name = "Спеціальність")]
        public int SpecialtyId { get; set; }
        public Specialty? Specialty { get; set; }

        [Required(ErrorMessage = "Це поле обов'язкове")]
        [StringLength(20, ErrorMessage = "Максимум 20 символів")]
        [Display(Name = "Кабінет")]
        public string Office { get; set; } = "";

        [Range(0, 60, ErrorMessage = "Стаж має бути від 0 до 60 років")]
        [Display(Name = "Стаж (років)")]
        public int Experience { get; set; }

        [StringLength(500, ErrorMessage = "Максимум 500 символів")]
        [Display(Name = "Опис")]
        public string? Description { get; set; }

        public List<Appointment> Appointments { get; set; } = new();
    }
}
