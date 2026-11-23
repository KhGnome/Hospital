using Hospital.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Головна сторінка: показуємо спеціальності та кількість лікарів
        public async Task<IActionResult> Index()
        {
            var specialties = await _context.Specialties
                .Include(s => s.Doctors)
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(specialties);
        }

        // Сторінка помилок (404, 500 тощо)
        public IActionResult Error(int? code)
        {
            ViewBag.Code = code;
            return View();
        }
    }
}
