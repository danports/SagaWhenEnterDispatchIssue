using Microsoft.EntityFrameworkCore;

namespace SagaWhenEnterDispatchIssue
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Rule> Rules { get; set; }
    }
}
