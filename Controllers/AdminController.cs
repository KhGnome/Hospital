using Hospital.Data;
using Hospital.Models;
using Hospital.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Controllers
{
    // Панель адміністратора
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ---------- Головна сторінка адміна ----------
        public async Task<IActionResult> Index()
        {
            ViewBag.SpecialtiesCount = await _context.Specialties.CountAsync();
            ViewBag.DoctorsCount = await _context.DoctorProfiles.CountAsync();
            ViewBag.AppointmentsCount = await _context.Appointments.CountAsync();
            ViewBag.PatientsCount = (await _userManager.GetUsersInRoleAsync("Patient")).Count;
            return View();
        }

        // ---------- Спеціальності ----------
        public async Task<IActionResult> Specialties()
        {
            var list = await _context.Specialties
                .Include(s => s.Doctors)
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSpecialty(string? name)
        {
            name = name?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                TempData["Error"] = "Введіть назву спеціальності";
                return RedirectToAction("Specialties");
            }
            if (name.Length > 50)
            {
                TempData["Error"] = "Назва спеціальності: максимум 50 символів";
                return RedirectToAction("Specialties");
            }

            // Перевірка на дублікат (порівнюємо в пам'яті, щоб не залежати від регістру кирилиці)
            var names = await _context.Specialties.Select(s => s.Name).ToListAsync();
            if (names.Any(n => string.Equals(n, name, StringComparison.CurrentCultureIgnoreCase)))
            {
                TempData["Error"] = "Така спеціальність уже існує";
                return RedirectToAction("Specialties");
            }

            _context.Specialties.Add(new Specialty { Name = name });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Спеціальність додано";
            return RedirectToAction("Specialties");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpecialty(int id)
        {
            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null) return NotFound();

            // Не можна видалити спеціальність, якщо є лікарі
            bool hasDoctors = await _context.DoctorProfiles.AnyAsync(d => d.SpecialtyId == id);
            if (hasDoctors)
            {
                TempData["Error"] = "Не можна видалити спеціальність, до якої прив'язані лікарі";
                return RedirectToAction("Specialties");
            }

            _context.Specialties.Remove(specialty);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Спеціальність видалено";
            return RedirectToAction("Specialties");
        }

        // ---------- Лікарі ----------
        public async Task<IActionResult> Doctors()
        {
            var doctors = await _context.DoctorProfiles
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .OrderBy(d => d.User!.FullName)
                .ToListAsync();
            return View(doctors);
        }

        [HttpGet]
        public async Task<IActionResult> CreateDoctor()
        {
            await LoadSpecialties();
            return View(new CreateDoctorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(CreateDoctorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadSpecialties(model.SpecialtyId);
                return View(model);
            }

            // Перевіряємо, що така спеціальність існує
            if (!await _context.Specialties.AnyAsync(s => s.Id == model.SpecialtyId))
            {
                ModelState.AddModelError("SpecialtyId", "Оберіть спеціальність");
                await LoadSpecialties();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                await LoadSpecialties(model.SpecialtyId);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Doctor");

            _context.DoctorProfiles.Add(new DoctorProfile
            {
                UserId = user.Id,
                SpecialtyId = model.SpecialtyId,
                Office = model.Office,
                Experience = model.Experience,
                Description = model.Description
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Акаунт лікаря створено";
            return RedirectToAction("Doctors");
        }

        // ---------- Усі записи ----------
        public async Task<IActionResult> Appointments()
        {
            var list = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor).ThenInclude(d => d!.User)
                .Include(a => a.Doctor).ThenInclude(d => d!.Specialty)
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
            return View(list);
        }

        // Список спеціальностей для випадаючого списку
        private async Task LoadSpecialties(int? selectedId = null)
        {
            var specialties = await _context.Specialties.OrderBy(s => s.Name).ToListAsync();
            ViewBag.Specialties = new SelectList(specialties, "Id", "Name", selectedId);
            ViewBag.HasSpecialties = specialties.Count > 0;
        }
    }
}
