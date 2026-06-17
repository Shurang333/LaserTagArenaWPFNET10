using System;
using System.IO;
using System.Windows;
using LaserTagArenaWPFNET10.Controllers;
using LaserTagArenaWPFNET10.Data;
using LaserTagArenaWPFNET10.Helpers;
using LaserTagArenaWPFNET10.Models;
using LaserTagArenaWPFNET10.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LaserTagArenaWPFNET10
{
    public partial class App : Application
    {
        public static ServiceProvider? ServiceProvider { get; private set; }  // <-- ИЗМЕНИТЕ НА ServiceProvider
        public static IConfiguration? Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Загрузка конфигурации
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // Настройка DI
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Инициализация БД
            LaserTagDatabaseInitializer.Initialize();

            // Глобальная обработка ошибок
            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show(ExceptionTranslator.Translate(args.Exception),
                    "Ошибка приложения", MessageBoxButton.OK, MessageBoxImage.Warning);
                args.Handled = true;
            };
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // DbContext
            services.AddDbContext<LaserTagDbContext>(options =>
                options.UseSqlServer(Configuration!.GetConnectionString("LaserTagDB")));

            // Контроллеры
            services.AddScoped<AuthController>();
            services.AddScoped<EquipmentController>();
            services.AddScoped<OrderController>();
            services.AddScoped<UserController>();
            services.AddScoped<CartController>();
            services.AddScoped<NotificationController>();
            services.AddScoped<PriceHistoryController>();
            services.AddScoped<RecommendationController>();
            services.AddScoped<ReferenceController>();
            services.AddScoped<ShipmentController>();

            // Сервисы
            services.AddScoped<NeuralNetworkService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<PdfReceiptService>();
            services.AddScoped<QrCodeService>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}