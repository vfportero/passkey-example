using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using passkey_example_backend.Data;

namespace passkey_example_backend.Endpoints;

public static class MakeAssertion
{
    public record MakeAssertionRequest (AuthenticatorAssertionRawResponse ClientResponse, string AssertionOptions);

    public class MakeAssertionResult : AssertionVerificationResult
    {
        public Guid UserId { get; set; }
    }

    public static async Task<IResult> Execute(
        [FromServices] IFido2 fido2,
        UserDb db,
        [FromBody] MakeAssertionRequest request)
    {
        try
        {
            // 1. Get the assertion options we sent the client
            var options = AssertionOptions.FromJson(request.AssertionOptions);

            // 2. Get registered credential from database
            var creds = db.UserCredentials.FirstOrDefault(c => c.Descriptor == request.ClientResponse.Id) ?? throw new Exception("Unknown credentials");

            // 3. Get credential counter from database
            var storedCounter = creds.SignatureCounter;

            // 4. Create callback to check if userhandle owns the credentialId
            IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cancellationToken) =>
            {
                var storedCreds = await db.UserCredentials.FirstOrDefaultAsync(c => c.UserHandle == args.UserHandle, cancellationToken);
                return storedCreds?.Descriptor.SequenceEqual(args.CredentialId) ?? false;
            };

            // 5. Make the assertion
            var res = await fido2.MakeAssertionAsync(request.ClientResponse, options, creds.PublicKey, storedCounter, callback);

            // // 6. Store the updated counter
            // DemoStorage.UpdateCounter(res.CredentialId, res.Counter);

            // 7. return OK to client
            return Results.Json(new MakeAssertionResult
            {
                Status = res.Status,
                ErrorMessage = res.ErrorMessage,
                CredentialId = res.CredentialId,
                Counter = res.Counter,
                UserId = creds.UserId
            });
        }
        catch (Exception e)
        {
            return Results.Json(new AssertionVerificationResult { Status = "error", ErrorMessage = e.Message });
        }
    }
}
