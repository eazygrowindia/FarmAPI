namespace FarmAPI.Models
{
    public enum UserRoles
    {
        FARMOWNER = 0,
        FARMHELP = 1,
        EASYGROWADMIN = 2,
        UNKNOWN = 3,
    }

    public enum SystemStatus
    {
        ACTIVE = 0,
        DEACTIVATED = 1,
        UNKNOWN = 2,
    }

    public static class UserRole
    {
        public static string ToRoleString(string? inputRoleString)
        {
            UserRoles role;
            if (Enum.TryParse<UserRoles>(inputRoleString, out role))
            {
                return role.ToString();
            }
            else
            {
                return UserRoles.UNKNOWN.ToString();
            }
        }
    }
}
