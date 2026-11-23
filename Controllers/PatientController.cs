using System.Globalization;
using System.Security.Claims;
using Hospital.Data;
using Hospital.Models;
using Hospital.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Controllers
{
    // Усі дії цього контролера доступні лише пацієнтам
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Id поточного користувача
        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public IActionResult Index()
        {
            return RedirectToAction("MyAppointments");
        }

        // ---------- Крок 1: вибір спеціальності ----------
        public async Task<IActionResult> ChooseSpecialty()
        {
            var specialties = await _context.Specialties
                .Include(s => s.Doctors)
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(specialties);
        }

        // ---------- Крок 2: вибір лікаря ----------
        public async Task<IActionResult> Doctors(int specialtyId)
        {
            var specialty = await _context.Specialties.FindAsync(specialtyId);
            if (specialty == null) return NotFound();

            var doctors = await _context.DoctorProfiles
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Where(d => d.SpecialtyId == specialtyId)
                .ToListAsync();

            ViewBag.SpecialtyName = specialty.Name;
            return View(doctors);
        }

        // ---------- Крок 3: вибір дати й часу ----------
        [HttpGet]
        public async Task<IActionResult> Book(int doctorId, DateTime? date)
        {
            var doctor = await LoadDoctor(doctorId);
            if (doctor == null) return NotFound();

            var model = new BookingViewModel
            {
                DoctorId = doctorId,
                Date = date?.Date ?? DateTime.Today
            };
            FillBookingModel(model, doctor);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookingViewModel model)
        {
            var doctor = await LoadDoctor(model.DoctorId);
            if (doctor == null) return NotFound();

            var date = model.Date.Date;

            // Не можна записатися на минулу дату
            if (date < DateTime.Today || date > DateTime.Today.AddDays(30))
                ModelState.AddModelError("Date", "Оберіть дату від сьогодні і не пізніше ніж через 30 днів");

            DateTime? chosen = null;
            if (ModelState.IsValid || !string.IsNullOrEmpty(model.Time))
            {
                if (date >= DateTime.Today && !string.IsNullOrEmpty(model.Time))
                {
                    // Перевіряємо вибраний час за списком слотів (зайнятий час і минулий час - IsBusy)
                    var slot = GetSlots(model.DoctorId, date).FirstOrDefault(s => s.Time == model.Time);
                    if (slot == null)
                        ModelState.AddModelError("Time", "Недійсний час");
                    else if (slot.IsBusy)
                        ModelState.AddModelError("Time", "Цей час уже зайнятий або минув. Оберіть інший");
                    else
                        chosen = date.Add(TimeSpan.Parse(slot.Time, CultureInfo.InvariantCulture));
                }
            }

            if (!ModelState.IsValid || chosen == null)
            {
                FillBookingModel(model, doctor);
                return View(model);
            }

            var appointment = new Appointment
            {
                PatientId = CurrentUserId,
                DoctorId = model.DoctorId,
                DateTime = chosen.Value,
                Complaint = model.Complaint,
                Status = AppointmentStatus.Pending,
                DateCreated = DateTime.Now
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ви успішно записалися до лікаря. Очікуйте підтвердження";
            return RedirectToAction("MyAppointments");
        }

        // ---------- Мої записи ----------
        public async Task<IActionResult> MyAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.Specialty)
                .Where(a => a.PatientId == CurrentUserId)
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
            return View(appointments);
        }

        // ---------- Скасування запису ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            // Шукаємо тільки серед записів поточного пацієнта
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == CurrentUserId);
            if (appointment == null) return NotFound();

            if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled)
            {
                TempData["Error"] = "Цей запис не можна скасувати";
                return RedirectToAction("MyAppointments");
            }

            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Запис скасовано";
            return RedirectToAction("MyAppointments");
        }

        // ---------- Сторінка прийому ----------
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.Specialty)
                .Include(a => a.Prescriptions)
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == CurrentUserId);
            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // ---------- Мої ліки / рецепти ----------
        public async Task<IActionResult> Prescriptions()
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Appointment).ThenInclude(a => a!.Doctor).ThenInclude(d => d!.User)
                .Where(p => p.Appointment!.PatientId == CurrentUserId)
                .OrderByDescending(p => p.Appointment!.DateTime)
                .ToListAsync();
            return View(prescriptions);
        }

        // ================= Допоміжні методи =================

        private async Task<DoctorProfile?> LoadDoctor(int doctorId)
        {
            return await _context.DoctorProfiles
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.Id == doctorId);
        }

        // Заповнює дані для відображення сторінки запису
        private void FillBookingModel(BookingViewModel model, DoctorProfile doctor)
        {
            if (model.Date < DateTime.Today) model.Date = DateTime.Today;
            if (model.Date > DateTime.Today.AddDays(30)) model.Date = DateTime.Today.AddDays(30);

            model.DoctorName = doctor.User?.FullName ?? "";
            model.SpecialtyName = doctor.Specialty?.Name ?? "";
            model.Office = doctor.Office;
            model.Slots = GetSlots(doctor.Id, model.Date);
        }

        // Слоти по 30 хвилин з 9:00 до 17:00 (останній слот о 16:30)
        private List<TimeSlot> GetSlots(int doctorId, DateTime date)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            // Час, який уже зайнятий (скасовані записи не рахуємо)
            var busyTimes = _context.Appointments
                .Where(a => a.DoctorId == doctorId
                         && a.Status != AppointmentStatus.Cancelled
                         && a.DateTime >= dayStart && a.DateTime < dayEnd)
                .Select(a => a.DateTime)
                .ToList();

            var slots = new List<TimeSlot>();
            for (var t = dayStart.AddHours(9); t < dayStart.AddHours(17); t = t.AddMinutes(30))
            {
                var current = t;
                bool isBusy = busyTimes.Any(b => b.Hour == current.Hour && b.Minute == current.Minute)
                              || current < DateTime.Now;
                slots.Add(new TimeSlot
                {
                    Time = current.ToString("HH:mm", CultureInfo.InvariantCulture),
                    IsBusy = isBusy
                });
            }
            return slots;
        }
    }
}
