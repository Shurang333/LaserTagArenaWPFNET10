using System;

namespace LaserTagArenaWPFNET10.Helpers
{
    /// <summary>
    /// Ошибка валидации или бизнес-логики с сообщением для пользователя на русском.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public static ValidationResult Ok() => new ValidationResult { IsValid = true };
        public static ValidationResult Error(string message) => new ValidationResult { IsValid = false, ErrorMessage = message };

        public void ThrowIfInvalid()
        {
            if (!IsValid)
                throw new ValidationException(ErrorMessage ?? "Ошибка валидации.");
        }
    }
}