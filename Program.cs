using System.Globalization;
using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. База даних SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity (користувачі й ролі) з простими правилами пароля
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<UkrainianIdentityErrorDescriber>(); // повідомлення українською

// 3. Куди відправляти неавторизованих
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// 4. MVC + українські стандартні повідомлення валідації
builder.Services.AddControllersWithViews(options =>
{
    // Не робити non-nullable string автоматично обов'язковим (з англійським текстом)
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

    var p = options.ModelBindingMessageProvider;
    p.SetValueMustNotBeNullAccessor(x => "Це поле обов'язкове");
    p.SetMissingBindRequiredValueAccessor(x => $"Відсутнє значення для поля {x}");
    p.SetMissingKeyOrValueAccessor(() => "Потрібно вказати значення");
    p.SetAttemptedValueIsInvalidAccessor((v, f) => $"Значення '{v}' некоректне для поля {f}");
    p.SetUnknownValueIsInvalidAccessor(f => $"Значення некоректне для поля {f}");
    p.SetValueIsInvalidAccessor(v => $"Значення '{v}' некоректне");
    p.SetValueMustBeANumberAccessor(f => $"Поле {f} має бути числом");
    p.SetNonPropertyAttemptedValueIsInvalidAccessor(v => $"Значення '{v}' некоректне");
    p.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Значення некоректне");
    p.SetNonPropertyValueMustBeANumberAccessor(() => "Значення має бути числом");
});

var app = builder.Build();

// 5. Культура uk-UA: дата ДД.ММ.РРРР, час 24 години
var ukCulture = new CultureInfo("uk-UA");
CultureInfo.DefaultThreadCurrentCulture = ukCulture;
CultureInfo.DefaultThreadCurrentUICulture = ukCulture;
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(ukCulture),
    SupportedCultures = new List<CultureInfo> { ukCulture },
    SupportedUICultures = new List<CultureInfo> { ukCulture }
};
localizationOptions.RequestCultureProviders.Clear(); // завжди uk-UA
app.UseRequestLocalization(localizationOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
// Сторінки помилок (404 тощо) українською
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 6. Заповнюємо базу тестовими даними
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

app.Run();
