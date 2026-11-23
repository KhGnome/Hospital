using System.ComponentModel.DataAnnotations;

namespace Hospital.Models
{
    // Статуси запису
    public enum AppointmentStatus
    {
        [Display(Name = "Очікує")]
        Pending = 0,

        [Display(Name = "Підтверджено")]
        Confirmed = 1,

        [Display(Name = "Завершено")]
        Completed = 2,

        [Display(Name = "Скасовано")]
        Cancelled = 3
    }

    // Допоміжні методи, щоб у представленнях показувати українську назву і колір статусу
    public static class AppointmentStatusExtensions
    {
        public static string GetName(this AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Pending => "Очікує",
                AppointmentStatus.Confirmed => "Підтверджено",
                AppointmentStatus.Completed => "Завершено",
                AppointmentStatus.Cancelled => "Скасовано",
                _ => ""
            };
        }

        public static string GetBadge(this AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Pending => "bg-warning text-dark",
                AppointmentStatus.Confirmed => "bg-primary",
                AppointmentStatus.Completed => "bg-success",
                AppointmentStatus.Cancelled => "bg-secondary",
                _ => "bg-light text-dark"
            };
        }
    }
}
