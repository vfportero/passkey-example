 using Microsoft.EntityFrameworkCore;
 using passkey_example_backend.Data;
 using passkey_example_backend.Endpoints;

 var builder = WebApplication.CreateBuilder(args);
 builder.Services.AddDbContext<UserDb>(options => options.UseInMemoryDatabase("users"));
 builder.Services.AddCors();
 builder.Services.AddFido2(builder.Configuration);
 builder.Services.AddSession();
 builder.Services.AddDistributedMemoryCache();
 var app = builder.Build();
 app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
 app.UseSession();

 app.MapGet("/", () => "Hello World!");
 app.MapGet("/users", ListUsers);
 app.MapGet("/users/{id}", GetUser);
 app.MapPost("/users", CreateUser);
 // app.MapPatch("/users/{id}", UpdateUser);

 app.MapPost("/makeCredentialOptions", MakeCredentialOptions.Execute);
 app.MapPost("/makeCredential", MakeCredential.Execute);

 static async Task<IResult> ListUsers(UserDb db)
 {
     var users = await db.Users.ToListAsync();
     return Results.Ok(users);
 }

 static async Task<IResult> GetUser(UserDb db, Guid id)
 {
     var user = await db.Users.FindAsync(id);
     if (user == null)
     {
         return Results.NotFound();
     }
     return Results.Ok(user);
 }

 static async Task<IResult> CreateUser(UserDb db, User user)
 {
     await db.Users.AddAsync(user);
     await db.SaveChangesAsync();
     return Results.Created($"/users/{user.Id}", user);
 }



    // static async Task<IResult> UpdateUser(IFido2 fido2, UserDb db, Guid id, AuthenticatorAttestationRawResponse attestationResponse)
    // {
    //     await fido2.MakeNewCredentialAsync(attestationResponse,
    //
    //     var existingUser = await db.Users.FindAsync(id);
    //     if (existingUser == null)
    //     {
    //         return Results.NotFound();
    //     }
    //     existingUser.Email = user.Email;
    //     await db.SaveChangesAsync();
    //     return Results.Ok(existingUser);
    // }


 app.Run();
