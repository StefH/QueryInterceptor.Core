using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;

namespace QueryInterceptor.Core.UnitTests.Helpers.Entities
{
    public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Post> Posts { get; set; }
    }
}