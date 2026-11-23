using Microsoft.AspNetCore.Identity;

namespace Hospital.Data
{
    // Переклад стандартних повідомлень Identity українською
    public class UkrainianIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() =>
            new IdentityError { Code = nameof(DefaultError), Description = "Сталася невідома помилка." };

        public override IdentityError ConcurrencyFailure() =>
            new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Дані було змінено іншим користувачем. Спробуйте ще раз." };

        public override IdentityError PasswordMismatch() =>
            new IdentityError { Code = nameof(PasswordMismatch), Description = "Неправильний пароль." };

        public override IdentityError InvalidToken() =>
            new IdentityError { Code = nameof(InvalidToken), Description = "Недійсний токен." };

        public override IdentityError LoginAlreadyAssociated() =>
            new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "Користувач з таким логіном уже існує." };

        public override IdentityError InvalidUserName(string? userName) =>
            new IdentityError { Code = nameof(InvalidUserName), Description = $"Логін '{userName}' некоректний: дозволені лише літери й цифри." };

        public override IdentityError InvalidEmail(string? email) =>
            new IdentityError { Code = nameof(InvalidEmail), Description = $"Email '{email}' некоректний." };

        public override IdentityError DuplicateUserName(string userName) =>
            new IdentityError { Code = nameof(DuplicateUserName), Description = $"Логін '{userName}' уже зайнятий." };

        public override IdentityError DuplicateEmail(string email) =>
            new IdentityError { Code = nameof(DuplicateEmail), Description = $"Email '{email}' уже зареєстровано." };

        public override IdentityError InvalidRoleName(string? role) =>
            new IdentityError { Code = nameof(InvalidRoleName), Description = $"Назва ролі '{role}' некоректна." };

        public override IdentityError DuplicateRoleName(string role) =>
            new IdentityError { Code = nameof(DuplicateRoleName), Description = $"Роль '{role}' уже існує." };

        public override IdentityError UserAlreadyHasPassword() =>
            new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "У користувача вже встановлено пароль." };

        public override IdentityError UserLockoutNotEnabled() =>
            new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "Блокування для цього користувача не ввімкнено." };

        public override IdentityError UserAlreadyInRole(string role) =>
            new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"Користувач уже має роль '{role}'." };

        public override IdentityError UserNotInRole(string role) =>
            new IdentityError { Code = nameof(UserNotInRole), Description = $"Користувач не має ролі '{role}'." };

        public override IdentityError PasswordTooShort(int length) =>
            new IdentityError { Code = nameof(PasswordTooShort), Description = $"Пароль має містити мінімум {length} символів." };

        public override IdentityError PasswordRequiresNonAlphanumeric() =>
            new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Пароль має містити хоча б один спеціальний символ." };

        public override IdentityError PasswordRequiresDigit() =>
            new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Пароль має містити хоча б одну цифру." };

        public override IdentityError PasswordRequiresLower() =>
            new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Пароль має містити хоча б одну малу літеру." };

        public override IdentityError PasswordRequiresUpper() =>
            new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Пароль має містити хоча б одну велику літеру." };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
            new IdentityError { Code = nameof(PasswordRequiresUniqueChars), Description = $"Пароль має містити мінімум {uniqueChars} різних символів." };

        public override IdentityError RecoveryCodeRedemptionFailed() =>
            new IdentityError { Code = nameof(RecoveryCodeRedemptionFailed), Description = "Не вдалося використати код відновлення." };
    }
}
