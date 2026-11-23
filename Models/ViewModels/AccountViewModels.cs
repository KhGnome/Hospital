using System.ComponentModel.DataAnnotations;

namespace Hospital.Models.ViewModels
{
    // Модель форми реєстрації пацієнта
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Це поле обов'язкове")]
        [StringLength(100, ErrorMessage = "Максимум 100 символів")]
        [Display(Name = "ПІБ")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Це поле обов'язкове")]
        [EmailAddress(ErrorMessage = "Некоректний email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Це поле обов'язкове")]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Телефон у форматі +380XXXXXXXXX")]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "Вкажіть дату народження")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата народження")]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "Це поле обов'язкове")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має містити мінімум 6 символів")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Підтвердіть пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролі не збігаються")]
        [Display(Name = "Підтвердження пароля")]
        public string ConfirmPassword { get; set; } = "";
    }

    // Модель форми входу
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Це поле обов'язкове")]
        [EmailAddress(ErrorMessage = "Некоректний email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Це поле обов'язкове")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = "";

        [Display(Name = "Запам'ятати мене")]
        public bool RememberMe { get; set; }
    }

    // Модель редагування профілю
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Це поле обов'язкове")]
        [StringLength(100, ErrorMessage = "Максимум 100 символів")]
        [Display(Name = "ПІБ")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Це поле обов'язкове")]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Телефон у форматі +380XXXXXXXXX")]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "Вкажіть дату народження")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата народження")]
        public DateTime? BirthDate { get; set; }
    }
}
