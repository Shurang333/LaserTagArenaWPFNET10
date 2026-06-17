using System;
using System.Linq;
using System.Text;
using LaserTagArenaWPFNET10.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LaserTagArenaWPFNET10.Helpers
{
    public static class ExceptionTranslator
    {
        public static string Translate(Exception ex)
        {
            if (ex is ValidationException ve)
                return ve.Message;

            Exception current = ex;
            var messages = new StringBuilder();

            while (current != null)
            {
                string translated = TranslateSingle(current);
                if (!string.IsNullOrWhiteSpace(translated) && messages.ToString().IndexOf(translated, StringComparison.Ordinal) < 0)
                {
                    if (messages.Length > 0) messages.AppendLine();
                    messages.Append(translated);
                }
                current = current.InnerException;
            }

            return messages.Length > 0
                ? messages.ToString()
                : "Произошла непредвиденная ошибка. Попробуйте ещё раз или обратитесь к администратору.";
        }

        private static string TranslateSingle(Exception ex)
        {
            if (ex is ValidationException ve)
                return ve.Message;

            if (ex is DbUpdateException dbUpdate)
            {
                if (dbUpdate.InnerException is SqlException exception)
                    return TranslateSqlException(exception);

                if (dbUpdate.InnerException != null)
                    return TranslateSingle(dbUpdate.InnerException);

                return "Ошибка при сохранении данных в базу.";
            }

            if (ex is SqlException sqlEx)
                return TranslateSqlException(sqlEx);

            string msg = ex.Message ?? "";

            if (msg.IndexOf("Cannot open database", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("не удается открыть базу", StringComparison.OrdinalIgnoreCase) >= 0)
                return "База данных «LaserTagArenaDB» не найдена.\nВыполните скрипт Database\\CreateDatabase.sql в SSMS (LocalDB).";

            if (msg.IndexOf("Login failed", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Не удалось подключиться к SQL Server. Проверьте, что LocalDB запущен.";

            if (msg.IndexOf("Multiplicity conflicts", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Ошибка конфигурации модели данных (связи между таблицами).\nОбратитесь к разработчику.";

            if (msg.IndexOf("validation errors were detected during model generation", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Ошибка построения модели Entity Framework.\nПроверьте связи между сущностями в LaserTagDbContext.";

            if (msg.IndexOf("duplicate key", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("UNIQUE KEY", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Запись с такими данными уже существует.";

            if (msg.IndexOf("REFERENCE constraint", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("DELETE statement conflicted", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Невозможно удалить: есть связанные записи (заказы, поставки и т.д.).";

            if (msg.IndexOf("INSERT statement conflicted", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Невозможно сохранить: указаны несуществующие связанные данные.";

            if (msg.IndexOf("A network-related", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Сетевая ошибка при подключении к базе данных.";

            if (msg.IndexOf("Timeout", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Превышено время ожидания ответа от базы данных.";

            if (msg.IndexOf("does not recognize the method", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("cannot be translated into a store expression", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Ошибка запроса к базе данных. Обратитесь к разработчику (метод не поддерживается в LINQ).";

            if (msg.IndexOf("Invalid object name", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (msg.IndexOf("Notifications", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Таблица «Notifications» не найдена.\nПерезапустите приложение или выполните Database\\UpdateDatabase.sql.";
                if (msg.IndexOf("CartItems", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Таблица «CartItems» не найдена.\nПерезапустите приложение или выполните Database\\UpdateDatabase.sql.";
                return "Таблица не найдена в базе данных.\nВыполните Database\\UpdateDatabase.sql в SSMS.";
            }

            if (ex is InvalidOperationException)
                return msg;

            if (msg.Length > 200)
                return msg.Substring(0, 197) + "...";

            return string.IsNullOrWhiteSpace(msg) ? null : msg;
        }

        public static bool IsMissingTable(Exception ex, string tableName)
        {
            Exception current = ex;
            while (current != null)
            {
                string msg = current.Message ?? "";
                if (msg.IndexOf("Invalid object name", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    msg.IndexOf(tableName, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                current = current.InnerException;
            }
            return false;
        }

        private static string TranslateSqlException(SqlException sqlError)
        {
            switch (sqlError.Number)
            {
                case 2:
                case 53:
                    return "SQL Server недоступен. Убедитесь, что LocalDB установлен и запущен.";
                case 4060:
                    return "База данных «LaserTagArenaDB» не найдена.\nВыполните Database\\CreateDatabase.sql.";
                case 18456:
                    return "Ошибка входа в SQL Server. Проверьте права доступа Windows.";
                case 2627:
                case 2601:
                    return "Запись с такими данными уже существует (дубликат ключа).";
                case 547:
                    if (sqlError.Message.IndexOf("DELETE", StringComparison.OrdinalIgnoreCase) >= 0)
                        return "Невозможно удалить: на эту запись ссылаются другие данные.";
                    return "Нарушение связи между таблицами. Проверьте выбранные значения.";
                case 515:
                    return "Не заполнены обязательные поля в базе данных.";
                case -2:
                    return "Превышено время ожидания запроса к базе данных.";
                default:
                    return $"Ошибка SQL Server (код {sqlError.Number}): {sqlError.Message}";
            }
        }

        private static string TranslateFieldName(string property)
        {
            return property switch
            {
                "LastName" => "Фамилия",
                "FirstName" => "Имя",
                "Phone" => "Телефон",
                "Email" => "Email",
                "PasswordHash" => "Пароль",
                "SKU" => "Артикул",
                "Name" => "Название",
                "CurrentPrice" => "Цена",
                "StockQuantity" => "Остаток",
                "DiscountPercent" => "Скидка",
                "OrderNumber" => "Номер заказа",
                _ => property
            };
        }
    }
}