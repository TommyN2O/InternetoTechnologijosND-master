using InternetoTechnologijosND.Controller;
using InternetoTechnologijosND.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// use RAM database
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDbContext<UserDb>(opt => opt.UseInMemoryDatabase("UserList"));

// use authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();

// fix how raw JSON is displayed in web
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.IncludeFields = true;
});

var app = builder.Build();

// use HTTPS
app.UseHttpsRedirection();

// set default startup page
app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = new List<string> { "Pages/login.html" } });

// group path start
RouteGroupBuilder todos = app.MapGroup("/todos"),
    users = app.MapGroup("/users");

// init API calls
todos.MapGet("/", TodoController.GetAllTodos).RequireAuthorization();
todos.MapGet("/completed", TodoController.GetCompletedTodos).RequireAuthorization();
todos.MapGet("/{id}", TodoController.GetTodoById).RequireAuthorization();
todos.MapPost("/", TodoController.CreateTodo).RequireAuthorization();
todos.MapPut("/{id}", TodoController.UpdateTodoById).RequireAuthorization();
todos.MapPut("/complete/{id}", TodoController.UpdateTodoToCompleteById).RequireAuthorization();
todos.MapDelete("/{id}", TodoController.DeleteTodoById).RequireAuthorization();

users.MapPost("/createToken", TodoController.CreateTokenAndUser);
users.MapGet("/", TodoController.GetAllUsers);
users.MapGet("/{id}", TodoController.GetUserById);
users.MapPut("/{id}", TodoController.UpdateUserById).RequireAuthorization();
users.MapDelete("/{id}", TodoController.DeleteUserById).RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();

// use wwwroot objects as file paths
app.UseStaticFiles();

app.Run();