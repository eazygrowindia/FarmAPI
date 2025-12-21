using FarmAPI.Models;
using FarmAPI.Services;
using FarmAPI.Services;
using FarmAPI.Utils;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace FarmAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _users;
        private readonly MagicLinkRepository _magicLinks;
        private readonly JwtService _jwt;
        private readonly Fido2Service _fido2;
        private readonly PasswordHasher _passwordHasher;

        public AuthController(UserRepository users, MagicLinkRepository magicLinks, JwtService jwt, Fido2Service fido2, PasswordHasher passwordHasher)
        {
            _users = users;
            _magicLinks = magicLinks;
            _jwt = jwt;
            _fido2 = fido2;
            _passwordHasher = passwordHasher;
        }

        public record RegisterRequest(string Mobile, string? Email, bool AsOwner, string? OwnerId);
        public record MagicRequest(string Email);
        public record MagicValidateRequest(string Token);
        public record PasskeyRegisterOptionsRequest(string UserId);
        public record PasskeyLoginOptionsRequest(string Mobile);
        public record PasswordRegisterRequest(string Name,string Mobile,string Password,string? Email);
        public record PasswordLoginRequest(string Mobile, string Password);

        [HttpPost("register-with-password")]
        public async Task<IActionResult> RegisterWithPassword([FromBody] PasswordRegisterRequest req)
        {
            // Registration based on contactNumber/mobile
            var existing = await _users.GetByMobileAsync(req.Mobile);
            if (existing != null)
                return BadRequest("Mobile already registered");

            User user = await _users.CreateUserAsync(req.Name, req.Mobile, req.Email);

            var (hash, salt) = _passwordHasher.HashPassword(req.Password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;

            // persist password fields
            await _users.UpdatePasswordAsync(user.UserId, hash, salt);

            var token = _jwt.CreateToken(user);

            // Set cookie EXACTLY like login
            Response.Cookies.Append("authToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,  //for localhost/http set false
                SameSite = SameSiteMode.None,   //For cross origin SameSiteMode.None, if same origin SameSiteMode.Strict
                Expires = DateTime.UtcNow.AddHours(8),
                Path = "/"
            });

            return Ok(new { message = "Registration successful", userId = user.UserId, mobile = user.Mobile });
        }

        // Add validation endpoint for Angular guards
        //[HttpGet("validate")]
        //public IActionResult Validate()
        //{
        //    Response.Cookies.Append("authToken", "", new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Secure = true,
        //        SameSite = SameSiteMode.Strict,
        //        Expires = DateTime.UtcNow.AddDays(-1) // Clear if invalid
        //    });

        //    return Ok(new { isValid = true });
        //}

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToArray();

            return Ok(new
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                mobile = User.Identity?.Name,
                roles
            });
        }


        [HttpGet("validate")]
        public IActionResult Validate()
        {
            Console.WriteLine($"   VALIDATE:");
            Console.WriteLine($"   Cookie: {Request.Cookies["authToken"]?.Substring(0, 20)}...");
            Console.WriteLine($"   IsAuthenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"   User: {User.Identity?.Name}");
            Console.WriteLine($"   Claims: {User.Claims.Count()}");
            // JWT middleware already validated cookie → User.Identity.IsAuthenticated = true/false
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated)
            {
                // Clear expired/invalid cookie
                ClearAuthCookie();
                return Unauthorized(new { isValid = false });
            }

            return Ok(new { isValid = true });
        }

        private void ClearAuthCookie()
        {
            Response.Cookies.Append("authToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // localhost = false
                SameSite = SameSiteMode.None,   //For cross origin SameSiteMode.None, if same origin SameSiteMode.Strict
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/"
            });
        }

        [HttpPost("login-with-password")]
        public async Task<IActionResult> LoginWithPassword([FromBody] PasswordLoginRequest req)
        {
            var user = await _users.GetByMobileAsync(req.Mobile);
            if (user is null || string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrEmpty(user.PasswordSalt) 
                || SystemStatusHelper.isDeactivated(user.SystemStatus))
                return Unauthorized("Invalid credentials");

            var ok = _passwordHasher.VerifyPassword(req.Password, user.PasswordHash, user.PasswordSalt);
            if (!ok)
                return Unauthorized("Invalid credentials");

            await _users.UpdateLastLoginAsync(user.UserId);
            var token = _jwt.CreateToken(user);

            Response.Cookies.Append("authToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // HTTPS only
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(8),
                Path = "/"
            });
            Console.WriteLine($"LOGIN: Set cookie {token.Substring(0, 20)}...");

            return Ok(new { message = "Login successful", userId = user.Id, mobile = user.Mobile });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Append("authToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1)
            });
            return Ok(new { message = "Logged out" });
        }

        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        //{
        //    // minimal example: either create owner or maintainer by mobile
        //    User user;
        //    if (req.AsOwner)
        //    {
        //        user = await _users.CreateOwnerAsync(req.Mobile, req.Email);
        //    }
        //    else
        //    {
        //        if (string.IsNullOrEmpty(req.OwnerId))
        //            return BadRequest("OwnerId required for non-owner");

        //        user = await _users.CreateMaintainerAsync(req.Mobile, req.OwnerId, req.Email);
        //    }

        //    var token = _jwt.CreateToken(user);
        //    return Ok(new { token, userId = user.Id, mobile = user.Mobile });
        //}

        // ----- PASSKEY REGISTRATION -----

        [HttpPost("passkey/register-options")]
        public async Task<IActionResult> GetPasskeyRegisterOptions([FromBody] PasskeyRegisterOptionsRequest req)
        {
            var options = await _fido2.GetRegisterOptionsAsync(req.UserId);
            return Ok(options);
        }

        [HttpPost("passkey/register")]
        public async Task<IActionResult> CompletePasskeyRegister(
            [FromBody] AuthenticatorAttestationRawResponse attestation)
        {
            // user id is encoded in attestation options; we just create credential and attach to user
            var cred = await _fido2.CompleteRegistrationAsync(attestation);

            // Cred includes CredentialId & PublicKey; we need to know which user
            // Id is in the Fido2User (options.User.Id) as bytes
            var json = HttpContext.Session.GetString("fido2.attestationOptions")
                ?? throw new InvalidOperationException("Missing options");
            var options = CredentialCreateOptions.FromJson(json);
            var userId = Encoding.UTF8.GetString(options.User.Id);

            await _users.AddWebAuthnCredentialAsync(userId, cred);
            var user = await _users.GetByUserIdAsync(userId)
                ?? throw new InvalidOperationException("User missing after credential add");

            var token = _jwt.CreateToken(user);

            Response.Cookies.Append("authToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(8),
                Path = "/"
            });

            return Ok(new { message = "Passkey registered", userId = user.UserId, mobile = user.Mobile });
        }

        // ----- PASSKEY LOGIN -----

        [HttpPost("passkey/login-options")]
        public async Task<IActionResult> GetPasskeyLoginOptions([FromBody] PasskeyLoginOptionsRequest req)
        {
            var options = await _fido2.GetLoginOptionsAsync(req.Mobile);
            return Ok(options);
        }

        [HttpPost("passkey/login")]
        public async Task<IActionResult> CompletePasskeyLogin(
            [FromBody] AuthenticatorAssertionRawResponse assertion)
        {
            var (user, _) = await _fido2.CompleteLoginAsync(assertion);
            await _users.UpdateLastLoginAsync(user.UserId);
            var token = _jwt.CreateToken(user);

            Response.Cookies.Append("authToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(8),
                Path = "/"
            });

            return Ok(new { message = "Passkey login successful", userId = user.Id, mobile = user.Mobile });
        }

        // ----- MAGIC LINK REQUEST -----
        //REVIEW: SMTP is not working has to be checked
        [HttpPost("magic/request")]
        public async Task<IActionResult> RequestMagicLink([FromBody] MagicRequest req, [FromServices] IEmailSender emailSender, [FromServices] IConfiguration cfg)
        {
            var user = await _users.GetByEmailAsync(req.Email);
            if (user is null || !user.EmailVerified)
            {
                // To avoid leaking which emails exist
                return Ok();
            }

            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-").Replace("/", "_").TrimEnd('=');

            var expiresAt = DateTime.UtcNow.AddMinutes(15);
            var magic = await _magicLinks.CreateAsync(user.Id, token, expiresAt);

            var baseUrl = cfg["App:BaseUrl"] ?? "https://localhost:4200";
            var magicUrl = $"{baseUrl}/auth/magic/callback?token={magic.Token}";

            var body = $"Click to sign in: {magicUrl}\nThis link expires in 15 minutes.";

            await emailSender.SendAsync(user.Email!, "Your sign-in link", body);

            return Ok();
        }

        // ----- MAGIC LINK VALIDATE -----

        [HttpPost("magic/validate")]
        public async Task<IActionResult> ValidateMagicLink([FromBody] MagicValidateRequest req)
        {
            var ml = await _magicLinks.GetValidByTokenAsync(req.Token);
            if (ml is null)
                return BadRequest("Invalid or expired link");

            await _magicLinks.MarkUsedAsync(ml.Id);

            var user = await _users.GetByIdAsync(ml.UserId);
            if (user is null) return BadRequest("User not found");

            await _users.UpdateLastLoginAsync(user.UserId);

            var jwt = _jwt.CreateToken(user);
            return Ok(new { token = jwt, userId = user.Id, mobile = user.Mobile });
        }

    }
}