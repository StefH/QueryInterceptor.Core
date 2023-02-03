using Microsoft.EntityFrameworkCore;

namespace QueryInterceptor.UnitTests.Helpers.Entities {
    public class BlogContext : DbContext {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        public BlogContext(DbContextOptions options) : base(options) { }
    }
}