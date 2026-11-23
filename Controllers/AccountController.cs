using Hospital.Data;
using Hospital.Models;
using Hospital.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // ---------- Реєстрація (тільки пацієнти) ----------
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.BirthDate.HasValue && model.BirthDate.Value.Date > DateTime.Today)
                ModelState.AddModelError("BirthDate", "Дата народження не може бути в майбутньому");

            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Patient");
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = "Реєстрація пройшла успішно. Ласкаво просимо!";
                return RedirectToAction("ChooseSpecialty", "Patient");
            }

            // Помилки Identity (вже українською завдяки нашому IdentityErrorDescriber)
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // ---------- Вхід ----------
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Неправильний email або пароль");
                return View(model);
            }

            TempData["Success"] = "Ви успішно увійшли в систему";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Перенаправляємо залежно від ролі
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Admin");
                if (await _userManager.IsInRoleAsync(user, "Doctor"))
                    return RedirectToAction("Index", "Doctor");
            }
            return RedirectToAction("MyAppointments", "Patient");
        }

        // ---------- Вихід ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ---------- Особистий кабінет ----------
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Якщо це лікар - покажемо ще й дані профілю лікаря
            ViewBag.DoctorProfile = await _context.DoctorProfiles
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            return View(user);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? "",
                BirthDate = user.BirthDate
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (model.BirthDate.HasValue && model.BirthDate.Value.Date > DateTime.Today)
                ModelState.AddModelError("BirthDate", "Дата народження не може бути в майбутньому");

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.BirthDate = model.BirthDate;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Дані профілю збережено";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        // ---------- Немає доступу ----------
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
