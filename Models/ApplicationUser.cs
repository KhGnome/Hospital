using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Hospital.Models
{
    // Користувач системи (пацієнт, лікар або адмін).
    // Email, пароль і PhoneNumber вже є в IdentityUser.
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Це поле обов'язкове")]
        [StringLength(100, ErrorMessage = "Максимум 100 символів")]
        [Display(Name = "ПІБ")]
        public string FullName { get; set; } = "";

        [Display(Name = "Дата народження")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
    }
}
