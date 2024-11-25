using InternetoTechnologijosND.Database;
using InternetoTechnologijosND.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InternetoTechnologijosND.Controller
{
    public class TodoController
    {
        public static async Task<IResult> CreateTodo(Todo todoItem, TodoDb todoDb, UserDb userDb, HttpContext context)
        {
            var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await userDb.Users.FirstOrDefaultAsync(usr => usr.Token == token);

            if (user is not null)
            {
                todoItem.UserId = user.Id;
                todoItem.IsComplete = false;

                todoDb.Todos.Add(todoItem);

                await todoDb.SaveChangesAsync();

                return TypedResults.Created($"/todos/{todoItem.Id}", todoItem);
            }

            return TypedResults.NotFound("No user found with given token");
        }

        public static async Task<IResult> GetAllTodos(TodoDb todoDb, UserDb userDb, HttpContext context)
        {
            var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await userDb.Users.FirstOrDefaultAsync(usr => usr.Token == token);

            if (user is not null)
            {
                var todos = await todoDb.Todos.Where(todo => todo.UserId == user.Id).ToArrayAsync();

                return todos.IsNullOrEmpty() ? TypedResults.NotFound("No todos") : TypedResults.Ok(todos);
            }

            return TypedResults.NotFound("No user found with given token");
        }

        public static async Task<IResult> GetCompletedTodos(TodoDb todoDb, UserDb userDb, HttpContext context)
        {
            var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await userDb.Users.FirstOrDefaultAsync(usr => usr.Token == token);

            if (user is not null)
            {
                var todos = await todoDb.Todos.Where(todo => todo.UserId == user.Id && todo.IsComplete).ToArrayAsync();

                return todos.IsNullOrEmpty() ? TypedResults.NotFound("No completed todos") : TypedResults.Ok(todos);
            }

            return TypedResults.NotFound("No user found with given token");
        }

        public static async Task<IResult> GetTodoById(int id, TodoDb todoDb, UserDb userDb, HttpContext context)
        {
            var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var user = await userDb.Users.FirstOrDefaultAsync(usr => usr.Token == token);

            if (user is not null)
            {
                var todo = await todoDb.Todos.FindAsync(id);

                if (todo is not null && todo.UserId == user.Id)
                {
                    return TypedResults.Ok(todo);
                }

                return TypedResults.NotFound("No todo found with given ID for the current user");
            }

            return TypedResults.NotFound("No user found with given token");
        }

        public static async Task<IResult> UpdateTodoById(int id, Todo todoInput, TodoDb todoDb, UserDb userDb, HttpContext context)
        {
            var todo = await todoDb.Todos.FindAsync(id);

            if (todo is not null)
            {
                var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await userDb.Users.FirstOrDefaultAsync(usr => usr.Token == token);

                if (user is not null)
                {
                    if (user.Id == todo.UserId)
                    {
                        todo.Name = todoInput.Name;

                        await todoDb.SaveChangesAsync();

                        return TypedResults.Ok(todo);
                    }

                    return TypedResults.NotFound("Todo belongs to another user");
                }

                return TypedResults.NotFound("No user found with given token");
            }

            return TypedResults.NotFound("No todo found with given ID");
        }

        public static async Task<IResult> UpdateTodoToCompleteById(int id, TodoDb todoDb, UserDb userDb, HttpContext context)
        {
            var todo = await todoDb.Todos.FindAsync(id);

            if (todo is not null)
            {
                var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await userDb.Users.FirstOrDefaultAsync(usr => usr.Token == token);

                if (user is not null)
                {
                    if (user.Id == todo.UserId)
                    {
                        todo.IsComplete = true;

                        await todoDb.SaveChangesAsync();

                        return TypedResults.Ok(todo);
                    }

                    return TypedResults.NotFound("Todo belongs to another user");
                }

                return TypedResults.NotFound("No user found with given token");
            }

            return TypedResults.NotFound("No todo found with given ID");
        }

        public static async Task<IResult> DeleteTodoById(int id, TodoDb todoDb, UserDb userDb, HttpContext context)
        {
            var todo = await todoDb.Todos.FindAsync(id);

            if (todo is not null)
            {
                var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                var user = await userDb.Users.FirstOrDefaultAsync(usr => usr.Token == token);

                if (user is not null)
                {
                    if (user.Id == todo.UserId)
                    {
                        if (todo.IsComplete == true)
                        {
                            todoDb.Todos.Remove(todo);

                            await todoDb.SaveChangesAsync();

                            return TypedResults.Ok("Todo deleted");
                        }

                        return TypedResults.NotFound("Todo is not completed");
                    }

                    return TypedResults.NotFound("Todo belongs to another user");
                }

                return TypedResults.NotFound("No user found with given token");
            }

            return TypedResults.NotFound("No todo found with given ID");
        }

        [AllowAnonymous]
        public static async Task<IResult> CreateTokenAndUser(User user, UserDb userDb, IConfiguration configuration)
        {
            if (user.Username == "joydip" && user.Password == "joydip123")
            {
                var issuer = configuration["Jwt:Issuer"];
                var audience = configuration["Jwt:Audience"];
                var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                        new Claim(JwtRegisteredClaimNames.Email, user.Username),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha512Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var stringToken = tokenHandler.WriteToken(token);

                user.Token = stringToken;
                userDb.Users.Add(user);
                await userDb.SaveChangesAsync();

                return TypedResults.Created($"/todos/{user.Id}", user);
            }

            return Results.Unauthorized();
        }

        public static async Task<IResult> GetAllUsers(UserDb userDb)
        {
            var users = await userDb.Users.ToArrayAsync();

            return users.IsNullOrEmpty() ? TypedResults.NotFound("No users") : TypedResults.Ok(users);
        }

        public static async Task<IResult> GetUserById(int id, UserDb userDb)
        {
            var user = await userDb.Users.FindAsync(id);

            return user is null ? TypedResults.NotFound("No user with given ID") : TypedResults.Ok(user);
        }

        public static async Task<IResult> UpdateUserById(int id, User userItem, UserDb userDb, HttpContext context)
        {
            var user = await userDb.Users.FindAsync(id);

            if (user is not null)
            {
                var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

                if (user.Token == token)
                {
                    user.Username = userItem.Username;
                    user.Password = userItem.Password;

                    await userDb.SaveChangesAsync();

                    return TypedResults.Ok(user);
                }

                return TypedResults.NotFound("Cannot update other user");
            }

            return TypedResults.NotFound("No user found with given ID");
        }

        public static async Task<IResult> DeleteUserById(int id, UserDb userDb, HttpContext context)
        {
            var user = await userDb.Users.FindAsync(id);

            if (user is not null)
            {
                var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

                if (user.Token == token)
                {
                    userDb.Users.Remove(user);

                    await userDb.SaveChangesAsync();

                    return TypedResults.Ok("User deleted");
                }

                return TypedResults.NotFound("Cannot delete other user");
            }

            return TypedResults.NotFound("No user found with given ID");
        }
    }
}