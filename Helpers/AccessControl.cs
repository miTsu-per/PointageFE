using System;
using System.Linq;
using System.Threading.Tasks;
using MobileApp.ReUse;
using Microsoft.Maui.Controls;

namespace MobileApp.Helpers
{
    public static class AccessControl
    {
        public static bool IsAuthorized(string requiredRole)
        {
            return UserSession.CurrentUser?.Role == requiredRole;
        }

        public static bool IsInRoles(params string[] allowedRoles)
        {
            var userRole = UserSession.CurrentUser?.Role;
            return allowedRoles.Contains(userRole);
        }

    }
}
