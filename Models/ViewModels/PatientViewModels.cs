using System.ComponentModel.DataAnnotations;

namespace Hospital.Models.ViewModels
{
    // Один часовий слот (30 хвилин)
    public class TimeSlot
    {
        public string Time { get; set; } = "";   // наприклад "09:30"
        public bool IsBusy { get; set; }         // зайнято або вже минуло
    }

    // Модель сторінки запису до лікаря
    public class BookingViewModel
    {
        public int DoctorId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Оберіть вільний час")]
        [Display(Name = "Час")]
        public string? Time { get; set; }

        [Required(ErrorMessage = "Опишіть, будь ласка, вашу скаргу")]
        [StringLength(1000, ErrorMessage = "Максимум 1000 символів")]
        [Display(Name = "Скарга")]
        public string Complaint { get; set; } = "";

        // Дані для відображення (не з форми)
        public string DoctorName { get; set; } = "";
        public string SpecialtyName { get; set; } = "";
        public string Office { get; set; } = "";
        public List<TimeSlot> Slots { get; set; } = new();
    }
}
