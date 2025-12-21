using FarmAPI.Models;
using FarmAPI.Services;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.Extensions.Options;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Base64Url = FarmAPI.Utils.Base64Url;

namespace FarmAPI.Services
{
    public class Fido2Service
    {
        private readonly Fido2 _fido2;
        private readonly UserRepository _users;
        private readonly IHttpContextAccessor _httpContext;

        public Fido2Service(
            UserRepository users,
            IHttpContextAccessor httpContext,
            IConfiguration config)
        {
            _users = users;
            _httpContext = httpContext;

            var origin = config["Fido2:Origin"] ?? "http://localhost:4200";
            var rpId = new Uri(origin).Host;

            _fido2 = new Fido2(new Fido2Configuration
            {
                ServerDomain = rpId,
                ServerName = "My Passwordless App",
                Origins = new HashSet<string> { origin }
            });
        }

        // ----- REGISTRATION (CREATE PASSKEY) -----

        public async Task<CredentialCreateOptions> GetRegisterOptionsAsync(string userId)
        {
            var user = await _users.GetByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found");

            var fidoUser = new Fido2User
            {
                Id = Encoding.UTF8.GetBytes(user.Id),
                Name = user.Mobile,
                DisplayName = user.Mobile
            };

            var existingCreds = user.WebAuthnCredentials
                .Select(c => new PublicKeyCredentialDescriptor(Base64Url.Decode(c.CredentialId)))
                .ToList();

            var authSelection = new AuthenticatorSelection
            {
                AuthenticatorAttachment = AuthenticatorAttachment.Platform, // fingerprint/Face ID
                UserVerification = UserVerificationRequirement.Required
            };

            var exts = new AuthenticationExtensionsClientInputs
            {
                UserVerificationMethod = true
            };

            var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = fidoUser,
                ExcludeCredentials = existingCreds,
                AuthenticatorSelection = authSelection,
                AttestationPreference = AttestationConveyancePreference.None,
                Extensions = exts
            });


            // store challenge in session for later verification
            _httpContext.HttpContext!.Session.SetString(
                "fido2.attestationOptions",
                options.ToJson());

            return options;
        }

        public async Task<WebAuthnCredential> CompleteRegistrationAsync(AuthenticatorAttestationRawResponse attestation)
        {
            var json = _httpContext.HttpContext!.Session.GetString("fido2.attestationOptions")
                ?? throw new InvalidOperationException("Missing attestation options");

            var options = CredentialCreateOptions.FromJson(json);

            IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, ct) =>
            {
                var existing = await _users.GetByCredentialIdAsync(Base64Url.Encode(args.CredentialId));
                return existing is null;
            };

            //var res = await _fido2.MakeNewCredentialAsync(attestation, options, callback);
            var res = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams { 
                AttestationResponse = attestation,
                OriginalOptions = options,
                IsCredentialIdUniqueToUserCallback = callback
            });

            var cred = new WebAuthnCredential
            {
                CredentialId = Base64Url.Encode(res.Id),
                PublicKey = Base64Url.Encode(res.PublicKey),
                SignCount = res.SignCount,
                CreatedAt = DateTime.UtcNow
            };

            return cred;
        }

        // ----- LOGIN (USE PASSKEY) -----

        public async Task<AssertionOptions> GetLoginOptionsAsync(string mobile)
        {
            var user = await _users.GetByMobileAsync(mobile)
                ?? throw new InvalidOperationException("User not found");

            if (user.WebAuthnCredentials.Count == 0)
                throw new InvalidOperationException("No passkeys registered");

            var existingCreds = user.WebAuthnCredentials
                .Select(c => new PublicKeyCredentialDescriptor(Base64Url.Decode(c.CredentialId)))
                .ToList();

            var exts = new AuthenticationExtensionsClientInputs
            {
                UserVerificationMethod = true
            };

            var options = _fido2.GetAssertionOptions(
                existingCreds,
                UserVerificationRequirement.Required,
                exts);

            _httpContext.HttpContext!.Session.SetString(
                "fido2.assertionOptions",
                options.ToJson());

            return options;
        }

        public async Task<(User user, long newCounter)> CompleteLoginAsync(AuthenticatorAssertionRawResponse assertion)
        {
            var json = _httpContext.HttpContext!.Session.GetString("fido2.assertionOptions")
                ?? throw new InvalidOperationException("Missing assertion options");

            var options = AssertionOptions.FromJson(json);

            // find user by credentialId
            //var credId = Base64Url.Encode(assertion.Id);
            var credId = assertion.Id;
            var user = await _users.GetByCredentialIdAsync(credId)
                ?? throw new InvalidOperationException("Unknown credential");

            // locate public key + counter
            var storedCred = user.WebAuthnCredentials
                .First(c => c.CredentialId == credId);

            //var res = await _fido2.MakeAssertionAsync(
            //    assertion,
            //    options,
            //    Base64Url.Decode(storedCred.PublicKey),
            //    storedCred.SignCount,
            //    (_, _) => Task.FromResult(true));
            //    var res = await _fido2.MakeAssertionAsync(new MakeAssertionParams {
            //        AssertionResponse = assertion,
            //        OriginalOptions = options,
            //        StoredPublicKey = Base64Url.Decode(storedCred.PublicKey),
            //        StoredSignatureCounter = (uint)storedCred.SignCount,
            //        (_, _) => Task.FromResult(true));
            //});
            var res = await _fido2.MakeAssertionAsync(new MakeAssertionParams
            {
                AssertionResponse = assertion,                         // AuthenticatorAssertionRawResponse
                OriginalOptions = options,                          // AssertionOptions
                StoredPublicKey = Base64Url.Decode(storedCred.PublicKey),
                StoredSignatureCounter = (uint)storedCred.SignCount,
                IsUserHandleOwnerOfCredentialIdCallback = (_, _) => Task.FromResult(true)
            });


            storedCred.SignCount = res.SignCount;
            storedCred.LastUsedAt = DateTime.UtcNow;
            await _users.AddWebAuthnCredentialAsync(user.UserId, storedCred); // simple update

            return (user, res.SignCount);
        }
    }
}