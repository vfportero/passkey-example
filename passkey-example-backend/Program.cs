 using Microsoft.EntityFrameworkCore;
 using passkey_example_backend.Data;

 var builder = WebApplication.CreateBuilder(args);
 builder.Services.AddDbContext<UserDb>(options => options.UseInMemoryDatabase("users"));
 builder.Services.AddCors();
 var app = builder.Build();
 app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

 app.MapGet("/", () => "Hello World!");
 app.MapGet("/users", ListUsers);
 app.MapGet("/users/{id}", GetUser);
 app.MapPost("/users", CreateUser);

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


 app.Run();
