using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    // Запис пацієнта до лікаря
    public class Appointment
    {
        public int Id { get; set; }

        public string PatientId { get; set; } = "";
        public ApplicationUser? Patient { get; set; }

        public int DoctorId { get; set; }
        public DoctorProfile? Doctor { get; set; }

        [Display(Name = "Дата і час")]
        public DateTime DateTime { get; set; }

        [Required(ErrorMessage = "Опишіть, будь ласка, вашу скаргу")]
        [StringLength(1000, ErrorMessage = "Максимум 1000 символів")]
        [Display(Name = "Скарга")]
        public string Complaint { get; set; } = "";

        [Display(Name = "Статус")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [StringLength(1000, ErrorMessage = "Максимум 1000 символів")]
        [Display(Name = "Коментар лікаря")]
        public string? DoctorComment { get; set; }

        [StringLength(300, ErrorMessage = "Максимум 300 символів")]
        [Display(Name = "Діагноз")]
        public string? Diagnosis { get; set; }

        [Display(Name = "Дата створення")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Призначення (ліки), які лікар виписав на цьому прийомі
        public List<Prescription> Prescriptions { get; set; } = new();
    }
}
