using System.Text;
using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using passkey_example_backend.Data;

namespace passkey_example_backend.Endpoints;

public static class MakeCredentialOptions
{
    public class MakeCredentialsRequest
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string AttType { get; set; }
        public string AuthType { get; set; }
        public string UserVerification { get; set; }
    }

    public static async Task<IResult> Execute(
        HttpContext httpContext,
        [FromServices] IFido2 fido2,
        UserDb db,
        [FromBody] MakeCredentialsRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.UserName))
            {
                request.UserName = $"{request.DisplayName} (Usernameless user created at {DateTime.UtcNow})";
            }

            // 1. Get user from DB by username (in our example, auto create missing users)
            var dbUser = db.Users.FirstOrDefault(x => x.Email == request.UserName);
            if (dbUser == null)
            {
                dbUser = new User { Email = request.UserName };
                db.Users.Add(dbUser);
                await db.SaveChangesAsync();
            }
            var user = new Fido2User
            {
                DisplayName = request.DisplayName,
                Name = request.UserName,
                Id = Encoding.UTF8.GetBytes(request.UserName) // byte representation of userID is required
            };

            // 2. Get user existing keys by username
            var existingKeys = dbUser.Credentials.Select(c => JsonSerializer.Deserialize<PublicKeyCredentialDescriptor>(c.DescriptorJson)).ToList();

            // 3. Create options
            var authenticatorSelection = new AuthenticatorSelection
            {
                // ResidentKey = residentKey.ToEnum<ResidentKeyRequirement>(),
                UserVerification = request.UserVerification.ToEnum<UserVerificationRequirement>()
            };

            if (!string.IsNullOrEmpty(request.AuthType))
            {
                authenticatorSelection.AuthenticatorAttachment = request.AuthType.ToEnum<AuthenticatorAttachment>();
            }

            var exts = new AuthenticationExtensionsClientInputs() { Extensions = true, UserVerificationMethod = true, };
            var options = fido2.RequestNewCredential(
                user,
                existingKeys,
                authenticatorSelection,
                request.AttType.ToEnum<AttestationConveyancePreference>(),
                exts);

            // 4. Temporarily store options, session/in-memory cache/redis/db
            httpContext.Session.SetString("fido2.attestationOptions", options.ToJson());

            // 5. return options to client
            return Results.Json(options);
        }
        catch (Exception e)
        {
            return Results.Json(new CredentialCreateOptions { Status = "error", ErrorMessage = e.Message });
        }
    }
}
