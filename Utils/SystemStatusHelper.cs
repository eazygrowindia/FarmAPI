using FarmAPI.Models;
using MongoDB.Driver;
namespace FarmAPI.Utils
{
    public static class SystemStatusHelper
    {
        private static readonly Dictionary<string, SystemStatus> StringToStatusMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["active"] = SystemStatus.ACTIVE,
                ["act"] = SystemStatus.ACTIVE,
                ["deactivated"] = SystemStatus.DEACTIVATED,
                ["deact"] = SystemStatus.DEACTIVATED,
                ["deactivate"] = SystemStatus.DEACTIVATED,
                [""] = SystemStatus.UNKNOWN
                //[null] = SystemStatus.UNKNOWN
            };

        public static SystemStatus GetStatus(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return SystemStatus.UNKNOWN;

            return StringToStatusMap.TryGetValue(s.Trim(), out var status)
                ? status
                : SystemStatus.UNKNOWN;
        }

        // For two-way conversion
        public static string ToDisplayString(SystemStatus status) => status switch
        {
            SystemStatus.ACTIVE => "Active",
            SystemStatus.DEACTIVATED => "Deactivated",
            _ => "Unknown"
        };

        public static bool isDeactivated(string? s) => GetStatus(s) switch
        {
            SystemStatus.DEACTIVATED => true,
            SystemStatus.UNKNOWN => true,
            _ => false
        };

        public static bool isActive(string? s) => GetStatus(s) switch
        {
            SystemStatus.ACTIVE => true,
            _ => false
        };
    }
}