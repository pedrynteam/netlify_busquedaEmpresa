using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MGTModel.AuthService;

namespace IdentityServer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        private static bool _created = false;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
           /*
           Database.EnsureDeleted();
           _created = false; // */

            if (!_created)
            {
                _created = true;
                Database.EnsureCreated();
            }
        }

        public DbSet<MGTModel.AuthService.UserApp> UserApp { get; set; }

    }
}
