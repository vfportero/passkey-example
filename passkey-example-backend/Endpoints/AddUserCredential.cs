using System.Text.Json;
using Fido2NetLib;
using Microsoft.AspNetCore.Mvc;
using passkey_example_backend.Data;

namespace passkey_example_backend.Endpoints;

public record AddUserCredentialRequest(AuthenticatorAttestationRawResponse AttestationResponse, string CredentialOptions);

public static class AddUserCredential
{

    public static async Task<IResult> Execute(
        // HttpContext httpContext,
        [FromServices] IFido2 fido2,
        UserDb db,
        [FromBody] AddUserCredentialRequest request
    )
    {
        try
        {
            // 1. get the options we sent the client
            // var credentialOptions = httpContext.Session.GetString("fido2.attestationOptions");
            var options = CredentialCreateOptions.FromJson(request.CredentialOptions);

            // 2. Create callback so that lib can verify credential id is unique to this user
            IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, cancellationToken) =>
            {
                // var users = db.Users.Where(
                //     u => u.Credentials.Any(
                //             c =>
                //             {
                //                 var descriptor = JsonSerializer.Deserialize<PublicKeyCredentialDescriptor>(c.DescriptorJson);
                //                 if (descriptor == null)
                //                 {
                //                     return false;
                //                 }
                //                 return descriptor.Id == args.CredentialId;
                //             }))
                //     .ToList();
                //
                // return !users.Any();

                return true;
            };

            // 2. Verify and make the credentials
            var success = await fido2.MakeNewCredentialAsync(request.AttestationResponse, options, callback);

            // 3. Store the credentials in db
            var dbUser = db.Users.FirstOrDefault(x => x.Email == success.Result.User.Name);

            if (dbUser == null)
            {
                return Results.Json((new Fido2.CredentialMakeResult(status: "error", errorMessage: $"User {success.Result.User.Name} not found", result: null)));
            }

            dbUser.Credentials.Add(new UserCredential()
            {
                User = dbUser,
                DescriptorJson = JsonSerializer.Serialize(success.Result.CredentialId),
                PublicKey = success.Result.PublicKey,
                UserHandle = success.Result.User.Id,
                SignatureCounter = success.Result.Counter,
                CredType = success.Result.CredType,
                RegDate = DateTime.Now,
                AaGuid = success.Result.Aaguid
            });

            await db.SaveChangesAsync();

            // Remove Certificates from success because System.Text.Json cannot serialize them properly. See https://github.com/passwordless-lib/fido2-net-lib/issues/328
            success.Result.AttestationCertificate = null;
            success.Result.AttestationCertificateChain = null;

            // 4. return "ok" to the client
            return Results.Json(success);
        }
        catch (Exception e)
        {
            return Results.Json((new Fido2.CredentialMakeResult(status: "error", errorMessage: e.Message, result: null)));
        }
    }
}
