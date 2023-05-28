using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using passkey_example_backend.Data;

namespace passkey_example_backend.Endpoints;

public static class MakeAssertionOptions
{
    public class MakeAssertionOptionsRequest
    {
        public string UserName { get; set; }
        public UserVerificationRequirement UserVerification { get; set; } = UserVerificationRequirement.Discouraged;
    }

    public static async Task<IResult> Execute(
        [FromServices] IFido2 fido2,
        UserDb db,
        [FromBody] MakeAssertionOptionsRequest request)
    {
        try
        {
            var existingCredentials = new List<PublicKeyCredentialDescriptor>();

            if (!string.IsNullOrEmpty(request.UserName))
            {
                // 1. Get user from DB
                var dbUser = db.Users.FirstOrDefault(x => x.Email == request.UserName) ?? throw new ArgumentException("Username was not registered");

                // 2. Get registered credentials from database
                existingCredentials = db.UserCredentials
                    .Where(c => c.UserId == dbUser.Id)
                    .Select(c => new PublicKeyCredentialDescriptor(c.Descriptor))
                    .ToList();
            }

            var exts = new AuthenticationExtensionsClientInputs()
            {
                UserVerificationMethod = true
            };

            // 3. Create options
            var options = fido2.GetAssertionOptions(
                existingCredentials,
                request.UserVerification,
                exts
            );

            // // 4. Temporarily store options, session/in-memory cache/redis/db
            // HttpContext.Session.SetString("fido2.assertionOptions", options.ToJson());

            // 5. Return options to client
            return Results.Json(options);
        }

        catch (Exception e)
        {
            return Results.Json(new AssertionOptions { Status = "error", ErrorMessage = e.Message });
        }
    }
}
