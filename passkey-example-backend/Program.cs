 using Microsoft.AspNetCore.Mvc;
 using Microsoft.EntityFrameworkCore;
 using passkey_example_backend.Data;
 using passkey_example_backend.Endpoints;

 var builder = WebApplication.CreateBuilder(args);
 builder.Services.AddDbContext<UserDb>(options => options.UseInMemoryDatabase("users"));
 builder.Services.AddCors();
 builder.Services.AddMemoryCache();
 builder.Services.AddDistributedMemoryCache();
 builder.Services.AddSession(options =>
 {
     options.IdleTimeout = TimeSpan.FromMinutes(2);
     options.Cookie.HttpOnly = true;
     options.Cookie.SameSite = SameSiteMode.Unspecified;
     options.Cookie.IsEssential = true;
 });
 builder.Services.AddFido2(
     options =>
     {
         options.ServerDomain = builder.Configuration["fido2:serverDomain"];
         options.ServerName = builder.Configuration["fido2:serverName"];
         options.Origins = builder.Configuration.GetSection("fido2:origins").Get<HashSet<string>>();
         options.TimestampDriftTolerance = builder.Configuration.GetValue<int>("fido2:timestampDriftTolerance");
         options.MDSCacheDirPath = builder.Configuration["fido2:MDSCacheDirPath"];
     }).AddCachedMetadataService(config =>
 {
     config.AddFidoMetadataRepository(httpClientBuilder =>
     {
         //TODO: any specific config you want for accessing the MDS
     });
 });
 builder.Services.AddApplicationInsightsTelemetry();
 var app = builder.Build();
 app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
 app.UseSession();

 app.MapGet("/", () => "Hello World!");
 app.MapGet("/users", QueryUsers);
 app.MapGet("/users/{id}", GetUser);
 app.MapGet("/users/{id}/credentials", GetUserCredentials);
 app.MapPost("/users", CreateUser);
 app.MapPost("/users/addCredential", AddUserCredential.Execute);
 app.MapPost("/makeCredentialOptions", MakeCredentialOptions.Execute);
 app.MapPost("/makeAssertionOptions", MakeAssertionOptions.Execute);


 static async Task<IResult> QueryUsers(UserDb db, [FromQuery] string? q = null)
 {
     List<User>? users;
     if (!string.IsNullOrEmpty(q))
     {
         users = await db.Users.Where(x => x.Email.Contains(q)).ToListAsync();
         return Results.Ok(users);
     }

     users = await db.Users.ToListAsync();
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

 static async Task<IResult> GetUserCredentials(UserDb db, Guid id)
 {
     var userCredentials = await db.UserCredentials.Where(x => x.UserId == id).ToListAsync();
     if (userCredentials?.Any() == false)
     {
         return Results.NotFound();
     }
     return Results.Ok(userCredentials);
 }

 static async Task<IResult> CreateUser(UserDb db, User user)
 {
     await db.Users.AddAsync(user);
     await db.SaveChangesAsync();
     return Results.Created($"/users/{user.Id}", user);
 }

 app.Run();
