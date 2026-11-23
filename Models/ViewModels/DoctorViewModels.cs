using System.ComponentModel.DataAnnotations;

namespace Hospital.Models.ViewModels
{
    // Один рядок призначення у формі завершення прийому
    public class PrescriptionInput
    {
        [StringLength(100, ErrorMessage = "Назва ліків: максимум 100 символів")]
        public string? MedicineName { get; set; }

        [StringLength(100, ErrorMessage = "Дозування: максимум 100 символів")]
        public string? Dosage { get; set; }

        [StringLength(200, ErrorMessage = "Як приймати: максимум 200 символів")]
        public string? HowToTake { get; set; }

        public int? Days { get; set; }
    }

    // Форма завершення прийому
    public class CompleteViewModel
    {
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "Вкажіть діагноз")]
        [StringLength(300, ErrorMessage = "Максимум 300 символів")]
        [Display(Name = "Діагноз")]
        public string Diagnosis { get; set; } = "";

        [Required(ErrorMessage = "Напишіть коментар")]
        [StringLength(1000, ErrorMessage = "Максимум 1000 символів")]
        [Display(Name = "Коментар лікаря")]
        public string DoctorComment { get; set; } = "";

        public List<PrescriptionInput> Prescriptions { get; set; } = new();

        // Для відображення
        public string PatientName { get; set; } = "";
        public string Complaint { get; set; } = "";
        public DateTime AppointmentDate { get; set; }
    }

    // Рядок у результатах пошуку пацієнта
    public class PatientSearchItem
    {
        public string PatientId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public int VisitsCount { get; set; }
        public DateTime LastVisit { get; set; }
    }
}
