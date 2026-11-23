using System.Security.Claims;
using Hospital.Data;
using Hospital.Models;
using Hospital.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Controllers
{
    // Кабінет лікаря. Лікар бачить тільки свої записи.
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Профіль лікаря, що зараз увійшов у систему
        private async Task<DoctorProfile?> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.DoctorProfiles
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }

        // Запис лише цього лікаря (чужі записи не знайдуться)
        private async Task<Appointment?> GetMyAppointment(int id, int doctorId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Prescriptions)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);
        }

        // ---------- Список записів + статистика + фільтри ----------
        public async Task<IActionResult> Index(AppointmentStatus? status, DateTime? date)
        {
            var doctor = await GetMyProfile();
            if (doctor == null) return Forbid();

            var query = _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id);

            if (date.HasValue)
            {
                // Фільтр за конкретною датою
                var start = date.Value.Date;
                var end = start.AddDays(1);
                query = query.Where(a => a.DateTime >= start && a.DateTime < end);
            }
            else
            {
                // За замовчуванням: сьогодні й найближчі дні
                var todayStart = DateTime.Today;
                query = query.Where(a => a.DateTime >= todayStart);
            }

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            var appointments = await query.OrderBy(a => a.DateTime).ToListAsync();

            // Статистика
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            int diff = ((int)today.DayOfWeek + 6) % 7;   // скільки днів минуло з понеділка
            var weekStart = today.AddDays(-diff);
            var weekEnd = weekStart.AddDays(7);

            ViewBag.TodayCount = await _context.Appointments.CountAsync(a =>
                a.DoctorId == doctor.Id && a.Status != AppointmentStatus.Cancelled
                && a.DateTime >= today && a.DateTime < tomorrow);

            ViewBag.WeekCount = await _context.Appointments.CountAsync(a =>
                a.DoctorId == doctor.Id && a.Status != AppointmentStatus.Cancelled
                && a.DateTime >= weekStart && a.DateTime < weekEnd);

            ViewBag.PatientsCount = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync();

            ViewData["Status"] = status;
            ViewData["Date"] = date;
            ViewBag.DoctorName = doctor.User?.FullName;
            return View(appointments);
        }

        // ---------- Сторінка запису: дані пацієнта і скарга ----------
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await GetMyProfile();
            if (doctor == null) return Forbid();

            var appointment = await GetMyAppointment(id, doctor.Id);
            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // ---------- Прийняти (підтвердити) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var doctor = await GetMyProfile();
            if (doctor == null) return Forbid();

            var appointment = await GetMyAppointment(id, doctor.Id);
            if (appointment == null) return NotFound();

            if (appointment.Status != AppointmentStatus.Pending)
            {
                TempData["Error"] = "Підтвердити можна лише запис зі статусом «Очікує»";
                return RedirectToAction("Index");
            }

            appointment.Status = AppointmentStatus.Confirmed;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Запис підтверджено";
            return RedirectToAction("Index");
        }

        // ---------- Відхилити ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var doctor = await GetMyProfile();
            if (doctor == null) return Forbid();

            var appointment = await GetMyAppointment(id, doctor.Id);
            if (appointment == null) return NotFound();

            if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Confirmed)
            {
                TempData["Error"] = "Цей запис уже не можна відхилити";
                return RedirectToAction("Index");
            }

            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Запис відхилено";
            return RedirectToAction("Index");
        }

        // ---------- Завершити прийом ----------
        [HttpGet]
        public async Task<IActionResult> Complete(int id)
        {
            var doctor = await GetMyProfile();
            if (doctor == null) return Forbid();

            var appointment = await GetMyAppointment(id, doctor.Id);
            if (appointment == null) return NotFound();

            if (appointment.Status != AppointmentStatus.Confirmed)
            {
                TempData["Error"] = "Завершити можна лише підтверджений прийом";
                return RedirectToAction("Details", new { id });
            }

            var model = new CompleteViewModel
            {
                AppointmentId = appointment.Id,
                PatientName = appointment.Patient?.FullName ?? "",
                Complaint = appointment.Complaint,
                AppointmentDate = appointment.DateTime
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(CompleteViewModel model)
        {
            var doctor = await GetMyProfile();
            if (doctor == null) return Forbid();

            var appointment = await GetMyAppointment(model.AppointmentId, doctor.Id);
            if (appointment == null) return NotFound();

            if (appointment.Status != AppointmentStatus.Confirmed)
            {
                TempData["Error"] = "Завершити можна лише підтверджений прийом";
                return RedirectToAction("Details", new { id = appointment.Id });
            }

            // Відбираємо заповнені рядки з ліками (повністю порожні пропускаємо)
            var filled = new List<PrescriptionInput>();
            foreach (var p in model.Prescriptions)
            {
                bool isEmpty = string.IsNullOrWhiteSpace(p.MedicineName)
                               && string.IsNullOrWhiteSpace(p.Dosage)
                               && string.IsNullOrWhiteSpace(p.HowToTake)
                               && p.Days == null;
                if (isEmpty) continue;

                bool isOk = !string.IsNullOrWhiteSpace(p.MedicineName)
                            && !string.IsNullOrWhiteSpace(p.Dosage)
                            && !string.IsNullOrWhiteSpace(p.HowToTake)
                            && p.Days != null && p.Days >= 1 && p.Days <= 365;
                if (!isOk)
                {
                    ModelState.AddModelError("", "Для кожних ліків заповніть усі поля (кількість днів від 1 до 365)");
                    break;
                }
                filled.Add(p);
            }

            if (!ModelState.IsValid)
            {
                model.PatientName = appointment.Patient?.FullName ?? "";
                model.Complaint = appointment.Complaint;
                model.AppointmentDate = appointment.DateTime;
                return View(model);
            }

            appointment.Diagnosis = model.Diagnosis;
            appointment.DoctorComment = model.DoctorComment;
            appointment.Status = AppointmentStatus.Completed;

            foreach (var p in filled)
            {
                _context.Prescriptions.Add(new Prescription
                {
                    AppointmentId = appointment.Id,
                    MedicineName = p.MedicineName!.Trim(),
                    Dosage = p.Dosage!.Trim(),
                    HowToTake = p.HowToTake!.Trim(),
                    Days = p.Days!.Value
                });
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = "Прийом завершено";
            return RedirectToAction("Index");
        }

        // ---------- Історія пацієнта у цього лікаря ----------
        public async Task<IActionResult> PatientHistory(string patientId)
        {
            var doctor = await GetMyProfile();
            if (doctor == null) return Forbid();

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Prescriptions)
                .Where(a => a.DoctorId == doctor.Id && a.PatientId == patientId)
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();

            // Якщо пацієнт ніколи не записувався до цього лікаря - нічого не показуємо
            if (appointments.Count == 0) return NotFound();

            ViewBag.Patient = appointments[0].Patient;

            // Минулі прийоми: дата вже минула або прийом завершено
            var history = appointments
                .Where(a => a.DateTime < DateTime.Now || a.Status == AppointmentStatus.Completed)
                .ToList();
            return View(history);
        }

        // ---------- Пошук пацієнта за ПІБ ----------
        public async Task<IActionResult> Search(string? query)
        {
            var doctor = await GetMyProfile();
            if (doctor == null) return Forbid();

            var all = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id)
                .ToListAsync();

            // Групуємо за пацієнтом (SQLite погано шукає без урахування регістру для кирилиці, тому фільтруємо в пам'яті)
            var patients = all
                .GroupBy(a => a.PatientId)
                .Select(g => new PatientSearchItem
                {
                    PatientId = g.Key,
                    FullName = g.First().Patient?.FullName ?? "",
                    PhoneNumber = g.First().Patient?.PhoneNumber,
                    BirthDate = g.First().Patient?.BirthDate,
                    VisitsCount = g.Count(),
                    LastVisit = g.Max(x => x.DateTime)
                });

            if (!string.IsNullOrWhiteSpace(query))
            {
                var text = query.Trim();
                patients = patients.Where(p => p.FullName.Contains(text, StringComparison.CurrentCultureIgnoreCase));
            }

            ViewBag.Query = query;
            return View(patients.OrderBy(p => p.FullName).ToList());
        }
    }
}
