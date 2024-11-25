using InternetoTechnologijosND.Objects;
using Microsoft.EntityFrameworkCore;

namespace InternetoTechnologijosND.Database
{
    public class TodoDb(DbContextOptions<TodoDb> options) : DbContext(options)
    {
        public DbSet<Todo> Todos => Set<Todo>();
    }
}