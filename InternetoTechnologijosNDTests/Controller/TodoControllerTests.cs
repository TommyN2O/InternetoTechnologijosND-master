using InternetoTechnologijosND.Database;
using InternetoTechnologijosND.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Cryptography;


namespace InternetoTechnologijosND.Controller
{
    [TestClass()]
    public class TodoControllerTests
    {
        [TestMethod()]
        public async Task CreateTodoTest()
        {
            var options = new DbContextOptionsBuilder<TodoDb>()
                .UseInMemoryDatabase(databaseName: "CreateTodoTest_TodoDb")
                .Options;

            var userOptions = new DbContextOptionsBuilder<UserDb>()
                .UseInMemoryDatabase(databaseName: "CreateTodoTest_UserDb")
                .Options;

            using var todoDb = new TodoDb(options);
            using var userDb = new UserDb(userOptions);

            var user = new User
            {
                Id = 1,
                Username = "joydip",
                Password = "joydip123",
                Token = "token"
            };

            userDb.Users.Add(user);
            await userDb.SaveChangesAsync();

            var todo = new Todo
            {
                Name = "Test Todo"
            };

            var mockContext = new DefaultHttpContext();
            mockContext.Request.Headers.Authorization = "Bearer token";

            var result = await TodoController.CreateTodo(todo, todoDb, userDb, mockContext);

            // check result type
            Assert.IsInstanceOfType(result, typeof(IResult));

            // check response code
            Assert.AreEqual(StatusCodes.Status200OK, mockContext.Response.StatusCode);

            // check createdTodo
            var createdTodo = await todoDb.Todos.FirstOrDefaultAsync(t => t.Name == "Test Todo");
            Assert.IsNotNull(createdTodo);
            Assert.AreEqual("Test Todo", createdTodo.Name);
            Assert.AreEqual(false, createdTodo.IsComplete);
            Assert.AreEqual(user.Id, createdTodo.UserId);
        }


        [TestMethod()]
        public async Task GetAllTodosTest()
        {
            var todoOptions = new DbContextOptionsBuilder<TodoDb>()
                .UseInMemoryDatabase(databaseName: "GetAllTodosTest_TodoDb")
                .Options;

            var userOptions = new DbContextOptionsBuilder<UserDb>()
                .UseInMemoryDatabase(databaseName: "GetAllTodosTest_UserDb")
                .Options;

            using var todoDb = new TodoDb(todoOptions);
            using var userDb = new UserDb(userOptions);

            var user = new User
            {
                Id = 1,
                Username = "joydip",
                Password = "joydip123",
                Token = "token"
            };
            userDb.Users.Add(user);
            await userDb.SaveChangesAsync();

            var todos = new List<Todo>
            {
                new() { Name = "Todo 1", IsComplete = false, UserId = user.Id },
                new() { Name = "Todo 2", IsComplete = true, UserId = user.Id }
            };
            todoDb.Todos.AddRange(todos);
            await todoDb.SaveChangesAsync();

            var mockContext = new DefaultHttpContext();
            mockContext.Request.Headers.Authorization = "Bearer token";

            var result = await TodoController.GetAllTodos(todoDb, userDb, mockContext);

            Assert.IsInstanceOfType(result, typeof(IResult));
            var okResult = (Ok<Todo[]>)result;
            var todosResult = okResult.Value;

            Assert.AreEqual(2, todosResult.Count());
            Assert.IsTrue(todosResult.Any(todo => todo.Name == "Todo 1"));
            Assert.IsTrue(todosResult.Any(todo => todo.Name == "Todo 2"));
        }

        [TestMethod()]
        public async Task GetCompletedTodosTest()
        {
            var todoOptions = new DbContextOptionsBuilder<TodoDb>()
                .UseInMemoryDatabase(databaseName: "GetCompletedTodosTest_TodoDb")
                .Options;

            var userOptions = new DbContextOptionsBuilder<UserDb>()
                .UseInMemoryDatabase(databaseName: "GetCompletedTodosTest_UserDb")
                .Options;

            using var todoDb = new TodoDb(todoOptions);
            using var userDb = new UserDb(userOptions);

            var user = new User
            {
                Id = 1,
                Username = "joydip",
                Password = "joydip123",
                Token = "token"
            };
            userDb.Users.Add(user);
            await userDb.SaveChangesAsync();

            var todos = new List<Todo>
            {
                new() { Name = "Todo 1", IsComplete = false, UserId = user.Id },
                new() { Name = "Todo 2", IsComplete = true, UserId = user.Id },
                new() { Name = "Todo 3", IsComplete = true, UserId = user.Id }
            };
            todoDb.Todos.AddRange(todos);
            await todoDb.SaveChangesAsync();

            var mockContext = new DefaultHttpContext();
            mockContext.Request.Headers.Authorization = "Bearer token";

            var result = await TodoController.GetCompletedTodos(todoDb, userDb, mockContext);

            Assert.IsInstanceOfType(result, typeof(Ok<Todo[]>));
            var okResult = (Ok<Todo[]>)result;
            var completedTodosResult = okResult.Value;

            Assert.AreEqual(2, completedTodosResult.Length);
            Assert.IsTrue(completedTodosResult.Any(todo => todo.Name == "Todo 2"));
            Assert.IsTrue(completedTodosResult.Any(todo => todo.Name == "Todo 3"));
        }

        [TestMethod()]
        public async Task GetTodoByIdTest()
        {
            var todoOptions = new DbContextOptionsBuilder<TodoDb>()
                .UseInMemoryDatabase(databaseName: "GetTodoByIdTest_TodoDb")
                .Options;

            var userOptions = new DbContextOptionsBuilder<UserDb>()
                .UseInMemoryDatabase(databaseName: "GetTodoByIdTest_UserDb")
                .Options;

            using var todoDb = new TodoDb(todoOptions);
            using var userDb = new UserDb(userOptions);

            var user = new User
            {
                Id = 1,
                Username = "joydip",
                Password = "joydip123",
                Token = "token"
            };
            userDb.Users.Add(user);
            await userDb.SaveChangesAsync();

            var todos = new List<Todo>
            {
                new() { Name = "Todo 1", IsComplete = true, UserId = user.Id },
                new() { Name = "Todo 2", IsComplete = false, UserId = user.Id },
                new() { Name = "Todo 3", IsComplete = true, UserId = user.Id }
            };
            todoDb.Todos.AddRange(todos);
            await todoDb.SaveChangesAsync();

            var mockContext = new DefaultHttpContext();
            mockContext.Request.Headers.Authorization = "Bearer token";

            var result = await TodoController.GetTodoById(2, todoDb, userDb, mockContext);

            Assert.IsInstanceOfType(result, typeof(Ok<Todo>));
            var okResult = (Ok<Todo>)result;
            var todoResult = okResult.Value;

            Assert.IsNotNull(todoResult);
            Assert.AreEqual(2, todoResult.Id);
            Assert.AreEqual("Todo 2", todoResult.Name);
            Assert.AreEqual(false, todoResult.IsComplete);
            Assert.AreEqual(user.Id, todoResult.UserId);
        }

        [TestMethod()]
        public async Task UpdateTodoByIdTest()
        {
            var todoOptions = new DbContextOptionsBuilder<TodoDb>()
                .UseInMemoryDatabase(databaseName: "UpdateTodoByIdTest_TodoDb")
                .Options;

            var userOptions = new DbContextOptionsBuilder<UserDb>()
                .UseInMemoryDatabase(databaseName: "UpdateTodoByIdTest_UserDb")
                .Options;

            using var todoDb = new TodoDb(todoOptions);
            using var userDb = new UserDb(userOptions);

            var user = new User
            {
                Id = 1,
                Username = "joydip",
                Password = "joydip123",
                Token = "token"
            };
            userDb.Users.Add(user);
            await userDb.SaveChangesAsync();

            var todo = new Todo
            {
                Id = 1,
                Name = "Original Todo",
                IsComplete = false,
                UserId = user.Id
            };
            todoDb.Todos.Add(todo);
            await todoDb.SaveChangesAsync();

            var mockContext = new DefaultHttpContext();
            mockContext.Request.Headers.Authorization = "Bearer token";

            var updatedTodo = new Todo
            {
                Id = 1,
                Name = "Updated Todo",
                IsComplete = false
            };

            var result = await TodoController.UpdateTodoById(1, updatedTodo, todoDb, userDb, mockContext);

            Assert.IsInstanceOfType(result, typeof(Ok<Todo>));
            var okResult = (Ok<Todo>)result;
            var todoResult = okResult.Value;

            Assert.IsNotNull(todoResult);
            Assert.AreEqual(1, todoResult.Id);
            Assert.AreEqual("Updated Todo", todoResult.Name);
            Assert.AreEqual(false, todoResult.IsComplete);
            Assert.AreEqual(user.Id, todoResult.UserId);

            var dbTodo = await todoDb.Todos.FirstOrDefaultAsync(t => t.Id == 1);
            Assert.IsNotNull(dbTodo);
            Assert.AreEqual("Updated Todo", dbTodo.Name);
            Assert.AreEqual(false, dbTodo.IsComplete);
            Assert.AreEqual(user.Id, dbTodo.UserId);
        }

        [TestMethod()]
        public async Task UpdateTodoToCompleteByIdTest()
        {
            var todoOptions = new DbContextOptionsBuilder<TodoDb>()
                .UseInMemoryDatabase(databaseName: "UpdateTodoToCompleteByIdTest_TodoDb")
                .Options;

            var userOptions = new DbContextOptionsBuilder<UserDb>()
                .UseInMemoryDatabase(databaseName: "UpdateTodoToCompleteByIdTest_UserDb")
                .Options;

            using var todoDb = new TodoDb(todoOptions);
            using var userDb = new UserDb(userOptions);

            var user = new User
            {
                Id = 1,
                Username = "joydip",
                Password = "joydip123",
                Token = "token"
            };
            userDb.Users.Add(user);
            await userDb.SaveChangesAsync();

            var todo = new Todo
            {
                Id = 1,
                Name = "Todo",
                IsComplete = false,
                UserId = user.Id
            };
            todoDb.Todos.Add(todo);
            await todoDb.SaveChangesAsync();

            var mockContext = new DefaultHttpContext();
            mockContext.Request.Headers.Authorization = "Bearer token";

            var result = await TodoController.UpdateTodoToCompleteById(1, todoDb, userDb, mockContext);

            Assert.IsInstanceOfType(result, typeof(Ok<Todo>));
            var okResult = (Ok<Todo>)result;
            var todoResult = okResult.Value;

            Assert.IsNotNull(todoResult);
            Assert.AreEqual(1, todoResult.Id);
            Assert.AreEqual("Todo", todoResult.Name);
            Assert.AreEqual(true, todoResult.IsComplete);
            Assert.AreEqual(user.Id, todoResult.UserId);

            var dbTodo = await todoDb.Todos.FirstOrDefaultAsync(t => t.Id == 1);
            Assert.IsNotNull(dbTodo);
            Assert.AreEqual("Todo", dbTodo.Name);
            Assert.AreEqual(true, dbTodo.IsComplete);
            Assert.AreEqual(user.Id, dbTodo.UserId);
        }

        [TestMethod()]
        public async Task DeleteTodoByIdTest()
        {
            var todoOptions = new DbContextOptionsBuilder<TodoDb>()
                .UseInMemoryDatabase(databaseName: "DeleteTodoByIdTest_TodoDb")
                .Options;

            var userOptions = new DbContextOptionsBuilder<UserDb>()
                .UseInMemoryDatabase(databaseName: "DeleteTodoByIdTest_UserDb")
                .Options;

            using var todoDb = new TodoDb(todoOptions);
            using var userDb = new UserDb(userOptions);

            var user = new User
            {
                Id = 1,
                Username = "joydip",
                Password = "joydip123",
                Token = "token"
            };
            userDb.Users.Add(user);
            await userDb.SaveChangesAsync();

            var todos = new List<Todo>
            {
                new() { Id = 1, Name = "Todo 1", IsComplete = false, UserId = user.Id },
                new() { Id = 2, Name = "Todo 2", IsComplete = true, UserId = user.Id }
            };
            todoDb.Todos.AddRange(todos);
            await todoDb.SaveChangesAsync();

            var mockContext = new DefaultHttpContext();
            mockContext.Request.Headers.Authorization = "Bearer token";

            var result = await TodoController.DeleteTodoById(2, todoDb, userDb, mockContext);

            Assert.IsInstanceOfType(result, typeof(Ok<string>));
            var okResult = (Ok<string>)result;
            var stringResult = okResult.Value;

            Assert.AreEqual("Todo deleted", stringResult);

            var deletedTodo = await todoDb.Todos.FirstOrDefaultAsync(t => t.Id == 2);
            Assert.IsNull(deletedTodo);

            var remainingTodo = await todoDb.Todos.FirstOrDefaultAsync(t => t.Id == 1);
            Assert.IsNotNull(remainingTodo);
            Assert.AreEqual("Todo 1", remainingTodo.Name);
        }

        [TestMethod()]
        public async Task CreateTokenAndUserTest()
        {
            var key = new byte[64];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
            configurationMock.Setup(c => c["Jwt:Key"]).Returns(Convert.ToBase64String(key));

            var userOptions = new DbContextOptionsBuilder<UserDb>()
                .UseInMemoryDatabase(databaseName: "CreateTokenAndUserTest_UserDb")
                .Options;

            using var userDb = new UserDb(userOptions);

            var newUser = new User
            {
                Username = "joydip",
                Password = "joydip123"
            };

            var result = await TodoController.CreateTokenAndUser(newUser, userDb, configurationMock.Object);

            Assert.IsInstanceOfType(result, typeof(Created<User>));

            var createdResult = (Created<User>)result;
            var createdUser = createdResult.Value;

            Assert.IsNotNull(createdUser);
            Assert.AreEqual("joydip", createdUser.Username);
            Assert.AreEqual("joydip123", createdUser.Password);
            Assert.IsNotNull(createdUser.Token);

            var dbUser = await userDb.Users.FirstOrDefaultAsync(u => u.Username == "joydip");
            Assert.IsNotNull(dbUser);
            Assert.AreEqual("joydip", dbUser.Username);
            Assert.AreEqual("joydip123", dbUser.Password);
            Assert.AreEqual(createdUser.Token, dbUser.Token);

        }

        [TestMethod()]
        public async Task GetAllUsersTest()
        {
            var userOptions = new DbContextOptionsBuilder<UserDb>()
                .UseInMemoryDatabase(databaseName: "GetAllUsersTest_UserDb")
                .Options;

            using var userDb = new UserDb(userOptions);

            var users = new List<User>
            {
                new() { Username = "user1", Password = "password1" },
                new() { Username = "user2", Password = "password2" },
                new() { Username = "user3", Password = "password3" }
            };

            userDb.Users.AddRange(users);
            await userDb.SaveChangesAsync();

            var result = await TodoController.GetAllUsers(userDb);

            if (users.Count > 0)
            {
                Assert.IsInstanceOfType(result, typeof(Ok<User[]>));

                var okResult = result as Ok<User[]>;
                Assert.IsNotNull(okResult);
                
                var userArray = okResult.Value;
                Assert.IsNotNull(userArray);
                Assert.AreEqual(users.Count, userArray.Length);

                foreach (var user in users)
                {
                    var dbUser = userArray.FirstOrDefault(u => u.Username == user.Username);
                    Assert.IsNotNull(dbUser);
                    Assert.AreEqual(user.Username, dbUser.Username);
                    Assert.AreEqual(user.Password, dbUser.Password);
                }
            }
            else
            {
                Assert.IsInstanceOfType(result, typeof(NotFound<string>));
                var notFoundResult = result as NotFound<string>;
                Assert.IsNotNull(notFoundResult);
                Assert.AreEqual("No users", notFoundResult.Value);
            }
        }

        [TestMethod()]
        public async Task GetUserByIdTest()
        {
            var userOptions = new DbContextOptionsBuilder<UserDb>()
                .UseInMemoryDatabase(databaseName: "GetUserByIdTest_UserDb")
                .Options;

            using var userDb = new UserDb(userOptions);

            var newUser = new User
            {
                Username = "joydip",
                Password = "joydip123"
            };

            userDb.Users.Add(newUser);
            await userDb.SaveChangesAsync();

            var result = await TodoController.GetUserById(newUser.Id, userDb);

            Assert.IsInstanceOfType(result, typeof(Ok<User>));

            var okResult = result as Ok<User>;
            Assert.IsNotNull(okResult);

            var userFromResult = okResult.Value;
            Assert.IsNotNull(userFromResult);
            Assert.AreEqual(newUser.Username, userFromResult.Username);
            Assert.AreEqual(newUser.Password, userFromResult.Password);

            var invalidResult = await TodoController.GetUserById(999, userDb);
            Assert.IsInstanceOfType(invalidResult, typeof(NotFound<string>));

            var notFoundResult = invalidResult as NotFound<string>;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("No user with given ID", notFoundResult.Value);
        }

        [TestMethod()]
        public async Task UpdateUserByIdTest()
        {
            var userOptions = new DbContextOptionsBuilder<UserDb>()
            .UseInMemoryDatabase(databaseName: "UpdateUserByIdTest_UserDb")
            .Options;

            using var userDb = new UserDb(userOptions);

            var user = new User
            {
                Id = 1,
                Username = "joydip",
                Password = "joydip123",
                Token = "token"
            };
            userDb.Users.Add(user);
            await userDb.SaveChangesAsync();

            var mockContext = new DefaultHttpContext();
            mockContext.Request.Headers.Authorization = "Bearer token";

            var updatedUser = new User
            {
                Id = 1,
                Username = "updated_joydip",
                Password = "new_password123"
            };

            var result = await TodoController.UpdateUserById(1, updatedUser, userDb, mockContext);

            Assert.IsInstanceOfType(result, typeof(Ok<User>));
            var okResult = (Ok<User>)result;
            var userResult = okResult.Value;

            Assert.IsNotNull(userResult);
            Assert.AreEqual(1, userResult.Id);
            Assert.AreEqual("updated_joydip", userResult.Username);
            Assert.AreEqual("new_password123", userResult.Password);
            Assert.AreEqual("token", userResult.Token);

            var dbUser = await userDb.Users.FirstOrDefaultAsync(u => u.Id == 1);
            Assert.IsNotNull(dbUser);
            Assert.AreEqual("updated_joydip", dbUser.Username);
            Assert.AreEqual("new_password123", dbUser.Password);
            Assert.AreEqual("token", dbUser.Token);
        }

        [TestMethod()]
        public async Task DeleteUserByIdTest()
        {
            var todoOptions = new DbContextOptionsBuilder<TodoDb>()
                .UseInMemoryDatabase(databaseName: "DeleteUserByIdTest_TodoDb")
                .Options;

            var userOptions = new DbContextOptionsBuilder<UserDb>()
                .UseInMemoryDatabase(databaseName: "DeleteUserByIdTest_UserDb")
                .Options;

            using var userDb = new UserDb(userOptions);

            var user = new User
            {
                Id = 1,
                Username = "joydip",
                Password = "joydip123",
                Token = "token"
            };
            userDb.Users.Add(user);
            await userDb.SaveChangesAsync();

            var mockContext = new DefaultHttpContext();
            mockContext.Request.Headers.Authorization = "Bearer token";

            var result = await TodoController.DeleteUserById(1, userDb, mockContext);

            Assert.IsInstanceOfType(result, typeof(Ok<string>));
            var okResult = (Ok<string>)result;
            var stringResult = okResult.Value;

            Assert.AreEqual("User deleted", stringResult);

            var deletedUser = await userDb.Users.FirstOrDefaultAsync(u => u.Id == 1);
            Assert.IsNull(deletedUser);
        }
    }
}