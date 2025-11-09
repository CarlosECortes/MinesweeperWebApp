using Microsoft.EntityFrameworkCore;
using MinesweeperWebApp.Models;

namespace MinesweeperWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor: passes options to the base DbContext class
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet represents a table in the database
        public DbSet<User> Users { get; set; }
    }
}
