using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using _521_Project_3.Models;

namespace _521_Project_3.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<_521_Project_3.Models.Movie> Movie { get; set; } = default!;
        public DbSet<_521_Project_3.Models.Actor> Actor { get; set; } = default!;
        public DbSet<_521_Project_3.Models.ActorMovie> ActorMovie { get; set; } = default!;
    }
}
