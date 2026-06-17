using System;
using System.Linq;
using System.Text.RegularExpressions;
using LaserTagArenaWPFNET10.Models;

namespace LaserTagArenaWPFNET10.Helpers
{
    public static class Validators
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[\w\.-]+@[\w\.-]+\.\w{2,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex PhoneRegex = new Regex(
            @"^(\+7|8)?[\s\-]?\(?\d{3}\)?[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}$", RegexOptions.Compiled);

        private static readonly Regex NameRegex = new Regex(
            @"^[а-яА-ЯёЁa-zA-Z\s\-']{2,100}$", RegexOptions.Compiled);

        private static readonly Regex SkuRegex = new Regex(
            @"^[a-zA-Z0-9\-_]{2,50}$", RegexOptions.Compiled);

        // ====== АВТОРИЗАЦИЯ ======

        public static ValidationResult ValidateLogin(string? login, string? password)
        {
            if (string.IsNullOrWhiteSpace(login))
                return ValidationResult.Error("Введите логин (email или телефон).");

            if (string.IsNullOrWhiteSpace(password))
                return ValidationResult.Error("Введите пароль.");

            if (password.Length < 4)
                return ValidationResult.Error("Пароль должен содержать минимум 4 символа.");

            if (login.Length > 100)
                return ValidationResult.Error("Логин слишком длинный (максимум 100 символов).");

            return ValidationResult.Ok();
        }

        public static ValidationResult ValidateRegistration(string lastName, string firstName, string phone, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                return ValidationResult.Error("Укажите фамилию.");

            if (string.IsNullOrWhiteSpace(firstName))
                return ValidationResult.Error("Укажите имя.");

            if (string.IsNullOrWhiteSpace(phone))
                return ValidationResult.Error("Укажите телефон.");

            if (string.IsNullOrWhiteSpace(email))
                return ValidationResult.Error("Укажите email.");

            if (string.IsNullOrWhiteSpace(password))
                return ValidationResult.Error("Укажите пароль.");

            lastName = lastName.Trim();
            firstName = firstName.Trim();
            phone = phone.Trim();
            email = email.Trim();

            if (!NameRegex.IsMatch(lastName))
                return ValidationResult.Error("Фамилия: только буквы, минимум 2 символа.");

            if (!NameRegex.IsMatch(firstName))
                return ValidationResult.Error("Имя: только буквы, минимум 2 символа.");

            if (!EmailRegex.IsMatch(email))
                return ValidationResult.Error("Некорректный формат email (пример: user@mail.ru).");

            if (email.Length > 100)
                return ValidationResult.Error("Email слишком длинный (максимум 100 символов).");

            if (!PhoneRegex.IsMatch(NormalizePhone(phone)))
                return ValidationResult.Error("Некорректный телефон (пример: +79001234567 или 89001234567).");

            if (password.Length < 4)
                return ValidationResult.Error("Пароль должен быть не короче 4 символов.");

            if (password.Length > 100)
                return ValidationResult.Error("Пароль слишком длинный (максимум 100 символов).");

            return ValidationResult.Ok();
        }

        // ====== ТОВАРЫ ======

        public static ValidationResult ValidateEquipment(Equipment equipment, bool isNew)
        {
            if (equipment == null)
                return ValidationResult.Error("Данные товара не указаны.");

            if (string.IsNullOrWhiteSpace(equipment.SKU))
                return ValidationResult.Error("Артикул не может быть пустым.");

            if (!SkuRegex.IsMatch(equipment.SKU.Trim()))
                return ValidationResult.Error("Артикул: только латиница, цифры, дефис (2–50 символов).");

            if (string.IsNullOrWhiteSpace(equipment.Name))
                return ValidationResult.Error("Название товара не может быть пустым.");

            if (equipment.Name.Trim().Length < 2)
                return ValidationResult.Error("Название товара слишком короткое (минимум 2 символа).");

            if (equipment.Name.Length > 200)
                return ValidationResult.Error("Название товара слишком длинное (максимум 200 символов).");

            if (equipment.CurrentPrice <= 0)
                return ValidationResult.Error("Цена должна быть больше 0.");

            if (equipment.CurrentPrice > 99_999_999)
                return ValidationResult.Error("Цена слишком большая.");

            if (equipment.OldPrice.HasValue && equipment.OldPrice.Value < 0)
                return ValidationResult.Error("Старая цена не может быть отрицательной.");

            if (equipment.OldPrice.HasValue && equipment.OldPrice.Value > 0 && equipment.OldPrice < equipment.CurrentPrice)
                return ValidationResult.Error("Старая цена не может быть меньше текущей.");

            if (equipment.DiscountPercent < 0 || equipment.DiscountPercent > 100)
                return ValidationResult.Error("Скидка должна быть от 0 до 100%.");

            if (equipment.StockQuantity < 0)
                return ValidationResult.Error("Остаток не может быть отрицательным.");

            if (equipment.StockQuantity > 1_000_000)
                return ValidationResult.Error("Остаток слишком большой.");

            if (!equipment.CategoryID.HasValue || equipment.CategoryID <= 0)
                return ValidationResult.Error("Выберите категорию товара.");

            if (!equipment.ManufacturerID.HasValue || equipment.ManufacturerID <= 0)
                return ValidationResult.Error("Выберите производителя.");

            if (!string.IsNullOrEmpty(equipment.Tags) && equipment.Tags.Length > 500)
                return ValidationResult.Error("Теги слишком длинные (максимум 500 символов).");

            if (!string.IsNullOrEmpty(equipment.Description) && equipment.Description.Length > 4000)
                return ValidationResult.Error("Описание слишком длинное (максимум 4000 символов).");

            return ValidationResult.Ok();
        }

        public static ValidationResult ValidateEquipmentForm(string sku, string name, string currentPriceText,
            string stockText, string discountText, string oldPriceText, object? categoryId, object? manufacturerId)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return ValidationResult.Error("Артикул не может быть пустым.");

            if (string.IsNullOrWhiteSpace(name))
                return ValidationResult.Error("Название не может быть пустым.");

            if (!decimal.TryParse(currentPriceText?.Replace(",", "."), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal price) || price <= 0)
                return ValidationResult.Error("Укажите корректную цену (число больше 0).");

            if (!int.TryParse(stockText, out int stock) || stock < 0)
                return ValidationResult.Error("Остаток должен быть целым числом ≥ 0.");

            if (!int.TryParse(discountText, out int discount) || discount < 0 || discount > 100)
                return ValidationResult.Error("Скидка должна быть целым числом от 0 до 100.");

            if (!string.IsNullOrWhiteSpace(oldPriceText) && oldPriceText != "0")
            {
                if (!decimal.TryParse(oldPriceText.Replace(",", "."), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal oldPrice) || oldPrice < 0)
                    return ValidationResult.Error("Старая цена должна быть числом ≥ 0.");

                if (oldPrice > 0 && oldPrice < price)
                    return ValidationResult.Error("Старая цена не может быть меньше текущей.");
            }

            if (categoryId == null)
                return ValidationResult.Error("Выберите категорию.");

            if (manufacturerId == null)
                return ValidationResult.Error("Выберите производителя.");

            return ValidationResult.Ok();
        }

        // ====== СПРАВОЧНИКИ ======

        public static ValidationResult ValidateReferenceName(string name, string fieldLabel = "Название")
        {
            if (string.IsNullOrWhiteSpace(name))
                return ValidationResult.Error($"{fieldLabel} не может быть пустым.");

            name = name.Trim();

            if (name.Length < 2)
                return ValidationResult.Error($"{fieldLabel} слишком короткое (минимум 2 символа).");

            if (name.Length > 100)
                return ValidationResult.Error($"{fieldLabel} слишком длинное (максимум 100 символов).");

            return ValidationResult.Ok();
        }

        // ====== КОРЗИНА / ЗАКАЗЫ ======

        public static ValidationResult ValidateCartAdd(int userId, int equipmentId, int quantity)
        {
            if (userId <= 0)
                return ValidationResult.Error("Необходима авторизация для добавления в корзину.");

            if (equipmentId <= 0)
                return ValidationResult.Error("Товар не выбран.");

            if (quantity <= 0)
                return ValidationResult.Error("Количество должно быть больше 0.");

            if (quantity > 9999)
                return ValidationResult.Error("Слишком большое количество (максимум 9999).");

            return ValidationResult.Ok();
        }

        public static ValidationResult ValidateOrderCheckout(int userId)
        {
            if (userId <= 0)
                return ValidationResult.Error("Для оформления заказа необходимо войти в систему.");

            return ValidationResult.Ok();
        }

        public static ValidationResult ValidateDeleteId(int id, string entityName)
        {
            if (id <= 0)
                return ValidationResult.Error($"Не выбран объект для удаления ({entityName}).");

            return ValidationResult.Ok();
        }

        public static ValidationResult ValidateSearchText(string? text, int maxLength = 100)
        {
            if (text != null && text.Length > maxLength)
                return ValidationResult.Error($"Слишком длинный поисковый запрос (максимум {maxLength} символов).");

            return ValidationResult.Ok();
        }

        public static string NormalizePhone(string phone) =>
            Regex.Replace(phone ?? "", @"[^\d+]", "");
    }
}