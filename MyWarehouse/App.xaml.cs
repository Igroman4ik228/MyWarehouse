using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyWarehouse.Models;
using MyWarehouse.Models.Entities;
using MyWarehouse.Pages;
using MyWarehouse.Services;
using MyWarehouse.Windows;
using System.Windows;

namespace MyWarehouse
{
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // Регистрируем DbContext с временем жизни Scoped (на уровне страницы)
            services.AddDbContext<AppDbContext>(ServiceLifetime.Scoped);

            // Регистрируем страницы
            services.AddTransient<HomePage>();
            services.AddTransient<TasksPage>();
            services.AddTransient<TasksHistoryPage>();
            services.AddTransient<ProductsPage>();
            services.AddTransient<ClientsPage>();
            services.AddTransient<LocationsPage>();

            services.AddScoped<ITaskProcessingService, TaskProcessingService>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}