using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    // Одне призначення (один препарат) в межах прийому
    public class Prescription
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        [Required(ErrorMessage = "Вкажіть назву ліків")]
        [StringLength(100, ErrorMessage = "Максимум 100 символів")]
        [Display(Name = "Назва ліків")]
        public string MedicineName { get; set; } = "";

        [Required(ErrorMessage = "Вкажіть дозування")]
        [StringLength(100, ErrorMessage = "Максимум 100 символів")]
        [Display(Name = "Дозування")]
        public string Dosage { get; set; } = "";

        [Required(ErrorMessage = "Вкажіть, як приймати")]
        [StringLength(200, ErrorMessage = "Максимум 200 символів")]
        [Display(Name = "Як приймати")]
        public string HowToTake { get; set; } = "";

        [Range(1, 365, ErrorMessage = "Кількість днів має бути від 1 до 365")]
        [Display(Name = "Кількість днів")]
        public int Days { get; set; }
    }
}
