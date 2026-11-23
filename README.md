# Лікарня: онлайн-запис до лікаря

Навчальний проєкт: ASP.NET Core MVC (.NET 8), Entity Framework Core, SQLServer, ASP.NET Core Identity, Bootstrap 5.

## Запуск

```bash
cd Hospital
dotnet restore
dotnet run
```

Відкрийте http://localhost:5080. База `hospital.db` створюється автоматично при першому запуску
разом з тестовими даними. Для Bootstrap і jQuery потрібен інтернет (підключаються через CDN).

### Міграції (необов'язково)

Без міграцій таблиці створюються через `EnsureCreated`. Якщо потрібні міграції:

```bash
dotnet tool install --global dotnet-ef --version 8.*
# спочатку видаліть файл hospital.db, якщо він уже є
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

## Тестові акаунти

| Роль    | Email                 | Пароль       |
|---------|-----------------------|--------------|
| Адмін   | admin@hospital.ua     | Admin123!    |
| Лікар 1 (терапевт)  | doctor1@hospital.ua | Doctor123! |
| Лікар 2 (хірург)    | doctor2@hospital.ua | Doctor123! |
| Лікар 3 (окуліст)   | doctor3@hospital.ua | Doctor123! |
| Лікар 4 (ЛОР)       | doctor4@hospital.ua | Doctor123! |
| Лікар 5 (невролог)  | doctor5@hospital.ua | Doctor123! |
| Пацієнт 1 | patient1@hospital.ua | Patient123! |
| Пацієнт 2 | patient2@hospital.ua | Patient123! |

## Структура

- `Models/` - сутності БД (`ApplicationUser`, `Specialty`, `DoctorProfile`, `Appointment`, `Prescription`) і `ViewModels/` для форм
- `Data/` - `ApplicationDbContext`, `DbInitializer` (seed), `UkrainianIdentityErrorDescriber`
- `Controllers/` - `Home`, `Account`, `Patient`, `Doctor`, `Admin` (уся логіка прямо в контролерах)
- `Views/` - Razor-представлення, `Shared/_Layout.cshtml` з меню за ролями
- `Program.cs` - налаштування Identity, культури uk-UA, маршрутів і seed

## Як це працює (для захисту)

- **Ролі.** `Patient`, `Doctor`, `Admin` зберігаються в таблицях Identity. Доступ до контролерів обмежено `[Authorize(Roles = "...")]`. Меню в `_Layout` показує пункти залежно від `User.IsInRole(...)`.
- **Реєстрація** створює тільки пацієнтів. Лікарів створює адмін (`Admin/CreateDoctor`): створюється користувач з роллю `Doctor` і `DoctorProfile` зі спеціальністю.
- **Запис до лікаря:** спеціальність -> лікар -> дата й слоти. Слоти по 30 хв з 9:00 до 16:30 (останній закінчується о 17:00) генерує метод `GetSlots`. Зайнятий час (записи не зі статусом «Скасовано») і минулий час недоступні. Те саме перевіряється на сервері при збереженні.
- **Безпека даних.** Пацієнт шукає записи за `PatientId == поточний користувач`, лікар за `DoctorId == свій профіль`. Чужий запис за Id не відкриється (повертається 404).
- **Статуси:** Очікує -> Підтверджено -> Завершено; Скасовано можна з «Очікує» і «Підтверджено». Скасований запис звільняє слот.
- **Завершення прийому:** лікар вводить діагноз, коментар і кілька ліків. Рядки ліків додає JavaScript, перед відправкою поля нумеруються `Prescriptions[i].Поле`, і MVC збирає їх у список.
- **Українська мова:** тексти в представленнях, `ErrorMessage` в атрибутах валідації, власний `IdentityErrorDescriber`, культура `uk-UA` у `Program.cs`, `<html lang="uk">`.
