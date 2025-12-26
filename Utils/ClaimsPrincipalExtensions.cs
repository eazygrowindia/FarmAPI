using System.Security.Claims;

namespace FarmAPI.Utils
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Retrieves LoggedIn UserId from the specified user principal.
        /// Actor stores LoggedIn UserId
        /// </summary>
        /// <param name="user">The user principal from which to retrieve the "Actor" claim. Cannot be null.</param>
        /// <returns>The value of the "Actor" claim if present; otherwise, null.</returns>
        public static string? GetActor(this ClaimsPrincipal user) =>
            user.FindFirst("Actor")?.Value ?? null;
    }
}
