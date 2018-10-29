using Microsoft.EntityFrameworkCore;
using Yggdrasil.EntityframeworkCore.Seeding.Migrations.Models;

namespace Yggdrasil.EntityframeworkCore.Seeding.Migrations.Contexts
{
    public class SeederDbContext : DbContext
    {
        public SeederDbContext(DbContextOptions<SeederDbContext> opts) : base(opts) { }

        public DbSet<User> Users { get; set; }
    }
}