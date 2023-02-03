using Microsoft.EntityFrameworkCore;

namespace QueryInterceptor.Core.ConsoleApp.Database {
    public class NorthwindModel : DbContext {
        public virtual DbSet<Car> Cars { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Cars;Trusted_Connection=True;");
        }
    }
}
