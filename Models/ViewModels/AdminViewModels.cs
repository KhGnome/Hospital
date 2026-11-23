using System.ComponentModel.DataAnnotations;

namespace Hospital.Models.ViewModels
{
    // Форма створення акаунта лікаря
    public class CreateDoctorViewModel
    {
        [Required(ErrorMessage = "Це поле обов'язкове")]
        [StringLength(100, ErrorMessage = "Максимум 100 символів")]
        [Display(Name = "ПІБ")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Це поле обов'язкове")]
        [EmailAddress(ErrorMessage = "Некоректний email")]
        [Display(Name = "Email (логін)")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Це поле обов'язкове")]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Телефон у форматі +380XXXXXXXXX")]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "Це поле обов'язкове")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має містити мінімум 6 символів")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = "";

        [Display(Name = "Спеціальність")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть спеціальність")]
        public int SpecialtyId { get; set; }

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
    }
}
