using InternetoTechnologijosND.Objects;
using Microsoft.EntityFrameworkCore;

namespace InternetoTechnologijosND.Database
{
    public class UserDb(DbContextOptions<UserDb> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
    }
}