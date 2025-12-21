using FarmAPI.Models;
using MongoDB.Driver;
namespace FarmAPI.Utils
{
    public static class UserRolesHelper
    {
        private static readonly Dictionary<string, UserRoles> StringToRoleMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // FARMOWNER variations
                ["farmowner"] = UserRoles.FARMOWNER,
                ["owner"] = UserRoles.FARMOWNER,
                ["farm_owner"] = UserRoles.FARMOWNER,
                ["fo"] = UserRoles.FARMOWNER,

                // FARMHELP variations
                ["farmhelp"] = UserRoles.FARMHELP,
                ["help"] = UserRoles.FARMHELP,
                ["farm_help"] = UserRoles.FARMHELP,
                ["maintainer"] = UserRoles.FARMHELP,
                ["fh"] = UserRoles.FARMHELP,

                // EASYGROWADMIN variations
                ["easygrowadmin"] = UserRoles.EASYGROWADMIN,
                ["admin"] = UserRoles.EASYGROWADMIN,
                ["ega"] = UserRoles.EASYGROWADMIN,
                ["superadmin"] = UserRoles.EASYGROWADMIN,

                // UNKNOWN
                [""] = UserRoles.UNKNOWN,
                //[null] = UserRoles.UNKNOWN
            };

        public static UserRoles GetRole(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return UserRoles.UNKNOWN;

            return StringToRoleMap.TryGetValue(s.Trim(), out var role)
                ? role
                : UserRoles.UNKNOWN;
        }

        public static string ToDisplayString(UserRoles role) => role switch
        {
            UserRoles.FARMOWNER => "FarmOwner",
            UserRoles.FARMHELP => "FarmHelp",
            UserRoles.EASYGROWADMIN => "EasyGrowAdmin",
            _ => "Unknown"
        };

        // For List<string> → List<UserRoles>
        public static List<UserRoles> GetRoles(List<string>? roleStrings)
        {
            if (roleStrings == null || !roleStrings.Any())
                return new List<UserRoles> { UserRoles.UNKNOWN };

            return roleStrings
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(GetRole)
                .Where(r => r != UserRoles.UNKNOWN)
                .Distinct()
                .ToList();
        }

        // Check if user has specific role
        public static bool HasRole(List<string> userRoles, UserRoles requiredRole)
        {
            var roleName = requiredRole.ToString();
            return userRoles.Any(r =>
                StringToRoleMap.ContainsKey(r) &&
                StringToRoleMap[r] == requiredRole);
        }
    }
}