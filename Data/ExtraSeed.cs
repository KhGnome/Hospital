using Hospital.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data
{
    // Додає ще лікарів і пацієнтів. Безпечно запускати щоразу:
    // кого вже додано (за email), того пропускає.
    public static class ExtraSeed
    {
        public static async Task AddMoreAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Знайти спеціальність за назвою або створити нову
            async Task<Specialty> GetSpecialty(string name)
            {
                var s = await context.Specialties.FirstOrDefaultAsync(x => x.Name == name);
                if (s == null)
                {
                    s = new Specialty { Name = name };
                    context.Specialties.Add(s);
                    await context.SaveChangesAsync();
                }
                return s;
            }

            // Створити користувача з роллю, якщо його ще немає
            async Task<ApplicationUser> EnsureUser(string email, string password,
                string fullName, string phone, DateTime birthDate, string role)
            {
                var existing = await userManager.FindByEmailAsync(email);
                if (existing != null) return existing;

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName,
                    PhoneNumber = phone,
                    BirthDate = birthDate
                };
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, role);
                return user;
            }

            // ----- Лікарі (пароль для всіх: Doctor123!) -----
            var doctors = new[]
            {
                new { Email = "doctor6@hospital.ua",  Name = "Гончаренко Віктор Павлович",  Phone = "+380671000006", Spec = "Терапевт",   Office = "102", Exp = 6,  Desc = "Лікар-терапевт. Консультації, оформлення довідок, диспансеризація." },
                new { Email = "doctor7@hospital.ua",  Name = "Кравченко Оксана Миколаївна", Phone = "+380671000007", Spec = "Хірург",     Office = "207", Exp = 11, Desc = "Хірург. Консультації та післяопераційний нагляд." },
                new { Email = "doctor8@hospital.ua",  Name = "Лисенко Андрій Романович",    Phone = "+380671000008", Spec = "Кардіолог",  Office = "401", Exp = 20, Desc = "Кардіолог вищої категорії. Тиск, аритмія, біль у серці." },
                new { Email = "doctor9@hospital.ua",  Name = "Остапенко Юлія Сергіївна",    Phone = "+380671000009", Spec = "Кардіолог",  Office = "403", Exp = 7,  Desc = "Кардіолог. ЕКГ, профілактика серцево-судинних захворювань." },
                new { Email = "doctor10@hospital.ua", Name = "Савченко Дмитро Ігорович",    Phone = "+380671000010", Spec = "Дерматолог", Office = "115", Exp = 9,  Desc = "Дерматолог. Висипи, алергічні реакції, захворювання шкіри." },
                new { Email = "doctor11@hospital.ua", Name = "Руденко Катерина Вікторівна", Phone = "+380671000011", Spec = "Педіатр",    Office = "120", Exp = 14, Desc = "Педіатр. Огляд дітей, щеплення, консультації для батьків." },
                new { Email = "doctor12@hospital.ua", Name = "Мороз Олександр Тарасович",   Phone = "+380671000012", Spec = "Окуліст",    Office = "311", Exp = 5,  Desc = "Офтальмолог. Діагностика зору, лікування запалень очей." },
                new { Email = "doctor13@hospital.ua", Name = "Павленко Інна Олегівна",      Phone = "+380671000013", Spec = "Невролог",   Office = "315", Exp = 16, Desc = "Невролог. Мігрені, запаморочення, остеохондроз." }
            };

            foreach (var d in doctors)
            {
                var user = await EnsureUser(d.Email, "Doctor123!", d.Name, d.Phone,
                    new DateTime(1980, 1, 15), "Doctor");

                var hasProfile = await context.DoctorProfiles.AnyAsync(p => p.UserId == user.Id);
                if (!hasProfile)
                {
                    var spec = await GetSpecialty(d.Spec);
                    context.DoctorProfiles.Add(new DoctorProfile
                    {
                        UserId = user.Id,
                        SpecialtyId = spec.Id,
                        Office = d.Office,
                        Experience = d.Exp,
                        Description = d.Desc
                    });
                    await context.SaveChangesAsync();
                }
            }

            // ----- Пацієнти (пароль для всіх: Patient123!) -----
            var patients = new[]
            {
                new { Email = "patient3@hospital.ua",  Name = "Коваль Ірина Андріївна",      Phone = "+380931000003", Born = new DateTime(1995, 2, 14) },
                new { Email = "patient4@hospital.ua",  Name = "Мартинюк Олег Петрович",      Phone = "+380931000004", Born = new DateTime(1983, 9, 3) },
                new { Email = "patient5@hospital.ua",  Name = "Захарченко Анна Вікторівна",  Phone = "+380931000005", Born = new DateTime(2001, 6, 21) },
                new { Email = "patient6@hospital.ua",  Name = "Дяченко Максим Олексійович",  Phone = "+380931000006", Born = new DateTime(1977, 12, 30) },
                new { Email = "patient7@hospital.ua",  Name = "Яковенко Світлана Ігорівна",  Phone = "+380931000007", Born = new DateTime(1990, 4, 17) },
                new { Email = "patient8@hospital.ua",  Name = "Гриценко Володимир Юрійович", Phone = "+380931000008", Born = new DateTime(1965, 8, 9) },
                new { Email = "patient9@hospital.ua",  Name = "Лук'яненко Тетяна Сергіївна", Phone = "+380931000009", Born = new DateTime(1998, 11, 5) },
                new { Email = "patient10@hospital.ua", Name = "Приходько Назар Богданович",  Phone = "+380931000010", Born = new DateTime(2003, 3, 27) },
                new { Email = "patient11@hospital.ua", Name = "Сорока Людмила Миколаївна",   Phone = "+380931000011", Born = new DateTime(1972, 7, 12) },
                new { Email = "patient12@hospital.ua", Name = "Данильченко Артем Вадимович", Phone = "+380931000012", Born = new DateTime(1988, 10, 1) }
            };

            foreach (var p in patients)
            {
                await EnsureUser(p.Email, "Patient123!", p.Name, p.Phone, p.Born, "Patient");
            }
        }
    }
}
