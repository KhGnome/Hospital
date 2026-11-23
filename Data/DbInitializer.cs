using Hospital.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data
{
    // Заповнює базу тестовими даними при першому запуску
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Створюємо базу: якщо є міграції - застосовуємо їх, якщо ні - створюємо таблиці напряму
            if (context.Database.GetMigrations().Any())
                await context.Database.MigrateAsync();
            else
                await context.Database.EnsureCreatedAsync();

            // 1. Ролі
            string[] roles = { "Patient", "Doctor", "Admin" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Якщо дані вже є, нічого не робимо
            if (context.Specialties.Any())
                return;

            // 2. Спеціальності
            var specialties = new List<Specialty>
            {
                new Specialty { Name = "Терапевт" },
                new Specialty { Name = "Хірург" },
                new Specialty { Name = "Окуліст" },
                new Specialty { Name = "ЛОР" },
                new Specialty { Name = "Невролог" }
            };
            context.Specialties.AddRange(specialties);
            await context.SaveChangesAsync();

            // Допоміжний метод: створює користувача з роллю
            async Task<ApplicationUser> CreateUser(string email, string password,
                string fullName, string phone, DateTime birthDate, string role)
            {
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

            // 3. Адмін
            await CreateUser("admin@hospital.ua", "Admin123!", "Коваленко Андрій Миколайович",
                "+380501112233", new DateTime(1985, 3, 12), "Admin");

            // 4. Лікарі (5 штук, по одному на спеціальність)
            var doctorsData = new[]
            {
                new { Email = "doctor1@hospital.ua", Name = "Іваненко Олена Петрівна",   Phone = "+380671000001", Spec = 0, Office = "101", Exp = 12, Desc = "Лікар-терапевт вищої категорії. Профілактика та лікування загальних захворювань." },
                new { Email = "doctor2@hospital.ua", Name = "Мельник Сергій Васильович", Phone = "+380671000002", Spec = 1, Office = "205", Exp = 18, Desc = "Хірург загального профілю. Проводить консультації та малі операції." },
                new { Email = "doctor3@hospital.ua", Name = "Шевченко Ірина Олексіївна", Phone = "+380671000003", Spec = 2, Office = "310", Exp = 8,  Desc = "Лікар-офтальмолог. Перевірка зору, підбір окулярів." },
                new { Email = "doctor4@hospital.ua", Name = "Бондаренко Тарас Іванович",  Phone = "+380671000004", Spec = 3, Office = "212", Exp = 15, Desc = "Отоларинголог. Лікування захворювань вуха, горла та носа." },
                new { Email = "doctor5@hospital.ua", Name = "Ткаченко Наталія Андріївна", Phone = "+380671000005", Spec = 4, Office = "314", Exp = 10, Desc = "Невролог. Головний біль, безсоння, болі в спині." }
            };

            var doctorProfiles = new List<DoctorProfile>();
            foreach (var d in doctorsData)
            {
                var user = await CreateUser(d.Email, "Doctor123!", d.Name, d.Phone,
                    new DateTime(1978, 5, 20), "Doctor");

                doctorProfiles.Add(new DoctorProfile
                {
                    UserId = user.Id,
                    SpecialtyId = specialties[d.Spec].Id,
                    Office = d.Office,
                    Experience = d.Exp,
                    Description = d.Desc
                });
            }
            context.DoctorProfiles.AddRange(doctorProfiles);
            await context.SaveChangesAsync();

            // 5. Тестові пацієнти
            var patient1 = await CreateUser("patient1@hospital.ua", "Patient123!",
                "Петренко Марія Степанівна", "+380931234567", new DateTime(1992, 7, 8), "Patient");
            var patient2 = await CreateUser("patient2@hospital.ua", "Patient123!",
                "Сидоренко Богдан Олегович", "+380991234568", new DateTime(1988, 11, 25), "Patient");

            // 6. Кілька записів для прикладу
            var today = DateTime.Today;

            // Минулий завершений прийом з призначеннями
            var done = new Appointment
            {
                PatientId = patient1.Id,
                DoctorId = doctorProfiles[0].Id,
                DateTime = today.AddDays(-7).AddHours(10),
                Complaint = "Кашель, підвищена температура 37.5 вже три дні.",
                Status = AppointmentStatus.Completed,
                Diagnosis = "Гострий бронхіт",
                DoctorComment = "Дотримуватися постільного режиму, пити більше рідини. Повторний огляд через тиждень.",
                DateCreated = today.AddDays(-9),
                Prescriptions = new List<Prescription>
                {
                    new Prescription { MedicineName = "Амбробене", Dosage = "30 мг", HowToTake = "По 1 таблетці 2 рази на день після їжі", Days = 7 },
                    new Prescription { MedicineName = "Парацетамол", Dosage = "500 мг", HowToTake = "При температурі вище 38, не більше 3 разів на день", Days = 5 }
                }
            };

            // Минулий скасований запис
            var cancelled = new Appointment
            {
                PatientId = patient2.Id,
                DoctorId = doctorProfiles[1].Id,
                DateTime = today.AddDays(-3).AddHours(11),
                Complaint = "Біль у коліні після тренування.",
                Status = AppointmentStatus.Cancelled,
                DateCreated = today.AddDays(-5)
            };

            // Майбутній підтверджений запис
            var confirmed = new Appointment
            {
                PatientId = patient1.Id,
                DoctorId = doctorProfiles[4].Id,
                DateTime = today.AddDays(1).AddHours(14),
                Complaint = "Часті головні болі, особливо ввечері.",
                Status = AppointmentStatus.Confirmed,
                DateCreated = today
            };

            // Майбутні записи, що очікують підтвердження
            var pending1 = new Appointment
            {
                PatientId = patient2.Id,
                DoctorId = doctorProfiles[0].Id,
                DateTime = today.AddDays(1).AddHours(9).AddMinutes(30),
                Complaint = "Загальна слабкість, швидка втомлюваність.",
                Status = AppointmentStatus.Pending,
                DateCreated = today
            };
            var pending2 = new Appointment
            {
                PatientId = patient1.Id,
                DoctorId = doctorProfiles[0].Id,
                DateTime = today.AddDays(2).AddHours(11),
                Complaint = "Контрольний огляд після бронхіту.",
                Status = AppointmentStatus.Pending,
                DateCreated = today
            };
            var pending3 = new Appointment
            {
                PatientId = patient2.Id,
                DoctorId = doctorProfiles[3].Id,
                DateTime = today.AddDays(3).AddHours(15),
                Complaint = "Закладений ніс і біль у горлі.",
                Status = AppointmentStatus.Pending,
                DateCreated = today
            };

            context.Appointments.AddRange(done, cancelled, confirmed, pending1, pending2, pending3);
            await context.SaveChangesAsync();
        }
    }
}
