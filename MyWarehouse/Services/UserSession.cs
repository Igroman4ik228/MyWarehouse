using MyWarehouse.Models;
using MyWarehouse.Models.Entities;

namespace MyWarehouse.Services
{
    internal static class UserSession
    {

        private static User? _currentUser;

        public static User CurrentUser
        {
            get => _currentUser ?? throw new InvalidOperationException("Пользователь не авторизован.");
            set => _currentUser = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static bool IsAdmin => CurrentUser.RoleId == (int)RoleType.Admin;
        public static bool IsManager => CurrentUser.RoleId == (int)RoleType.Manager;
        public static bool IsCourier => CurrentUser.RoleId == (int)RoleType.Courier;
    }
}
