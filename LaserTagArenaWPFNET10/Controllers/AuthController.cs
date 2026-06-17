using LaserTagArenaWPFNET10.Data;
using LaserTagArenaWPFNET10.Helpers;
using LaserTagArenaWPFNET10.Models;
using LaserTagArenaWPFNET10.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LaserTagArenaWPFNET10.Controllers
{
    public class AuthController
    {
        private readonly LaserTagDbContext _db;
        private readonly NotificationService _notifications;

        public AuthController(LaserTagDbContext db, NotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public LoginResult Login(string login, string password)
        {
            var validation = Validators.ValidateLogin(login, password);
            if (!validation.IsValid)
                return LoginResult.Fail(validation.ErrorMessage);

            try
            {
                string passwordHash = HashPassword(password);
                login = login.Trim();
                string normalizedLogin = Validators.NormalizePhone(login);

                var user = _db.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u =>
                        (u.Email == login || u.Phone == login || u.Phone == normalizedLogin) &&
                        u.PasswordHash == passwordHash &&
                        u.Status != false);

                if (user == null)
                    return LoginResult.Fail("Неверный логин или пароль.");

                if (user.Status == false)
                    return LoginResult.Fail("Учётная запись заблокирована. Обратитесь к администратору.");

                if (user.RoleID != UserRoles.Admin && user.RoleID != UserRoles.Manager)
                    return LoginResult.Fail("Доступ запрещён. Допускаются только менеджеры и администраторы.");

                ApplicationSession.CurrentUser = user;
                return LoginResult.Ok(user);
            }
            catch (Exception ex)
            {
                return LoginResult.Fail(ExceptionTranslator.Translate(ex));
            }
        }

        public RegisterResult Register(string lastName, string firstName, string phone, string email, string password)
        {
            var validation = Validators.ValidateRegistration(lastName, firstName, phone, email, password);
            if (!validation.IsValid)
                return RegisterResult.Fail(validation.ErrorMessage);

            try
            {
                email = email.Trim().ToLower();
                phone = Validators.NormalizePhone(phone.Trim());

                if (_db.Users.Any(u => u.Email == email))
                    return RegisterResult.Fail("Пользователь с таким email уже зарегистрирован.");

                bool phoneExists = _db.Users
                    .Select(u => u.Phone)
                    .AsEnumerable()
                    .Any(p => Validators.NormalizePhone(p) == phone);
                if (phoneExists)
                    return RegisterResult.Fail("Пользователь с таким телефоном уже зарегистрирован.");

                if (!_db.Roles.Any(r => r.RoleID == UserRoles.Manager))
                    return RegisterResult.Fail("Роль «Менеджер» не найдена в базе. Выполните скрипт CreateDatabase.sql.");

                _db.Users.Add(new User
                {
                    LastName = lastName.Trim(),
                    FirstName = firstName.Trim(),
                    Phone = phone,
                    Email = email,
                    PasswordHash = HashPassword(password),
                    RegistrationDate = DateTime.Now,
                    Status = true,
                    Discount = 0,
                    RoleID = UserRoles.Manager
                });
                _db.SaveChanges();

                _notifications.NotifyAdmins(
                    $"Зарегистрирован менеджер: {lastName.Trim()} {firstName.Trim()} ({email})",
                    NotificationTypes.UserRegister);

                return RegisterResult.Ok("Регистрация успешна. Войдите, используя email и пароль.");
            }
            catch (Exception ex)
            {
                return RegisterResult.Fail(ExceptionTranslator.Translate(ex));
            }
        }

        public void Logout() => ApplicationSession.Clear();

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public User? User { get; set; }

        public static LoginResult Ok(User user) =>
            new LoginResult { Success = true, User = user, Message = $"Добро пожаловать, {user.FullName}!" };

        public static LoginResult Fail(string message) =>
            new LoginResult { Success = false, Message = message };
    }

    public class RegisterResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public static RegisterResult Ok(string message) => new RegisterResult { Success = true, Message = message };
        public static RegisterResult Fail(string message) => new RegisterResult { Success = false, Message = message };
    }
}