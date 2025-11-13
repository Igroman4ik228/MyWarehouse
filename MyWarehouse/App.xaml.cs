using Microsoft.Extensions.DependencyInjection;
using MyWarehouse.Models.Entities;
using MyWarehouse.Models.ViewModels;
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
            services.AddDbContext<AppDbContext>(ServiceLifetime.Scoped);

            // Services
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<ITaskProcessingService, TaskProcessingService>();
            services.AddTransient<IProductService, ProductService>();

            services.AddTransient<ProductsPageViewModel>();

            services.AddTransient<AdminPanelWindow>();

            // Регистрируем страницы
            services.AddTransient<LoginPage>();
            services.AddTransient<HomePage>();
            services.AddTransient<TasksPage>();
            services.AddTransient<TasksHistoryPage>();
            services.AddTransient<ProductsPage>();
            services.AddTransient<ClientsPage>();
            services.AddTransient<LocationsPage>();

        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}