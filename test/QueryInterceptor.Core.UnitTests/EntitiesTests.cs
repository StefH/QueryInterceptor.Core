using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity;
using QueryInterceptor.Core.ExpressionVisitors;
using QueryInterceptor.Core.UnitTests.Helpers.Entities;
using TestToolsToXunitProxy;
using Xunit;
using Assert = Xunit.Assert;

namespace QueryInterceptor.Core.UnitTests
{
    /// <summary>
    /// Summary description for EntitiesTests
    /// </summary>
    public class EntitiesTests : IDisposable
    {
        static readonly Random Rnd = new Random(1);

        BlogContext _context;

        public EntitiesTests()
        {
            var builder = new DbContextOptionsBuilder();
            //builder.UseSqlite($"Filename=QueryInterceptor.Core.DB.{Guid.NewGuid()}.db");
            builder.UseInMemoryDatabase();

            _context = new BlogContext(builder.Options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        // Use TestCleanup to run code after each test has run
        public void Dispose()
        {
            _context.Database.EnsureDeleted();

            _context.Dispose();
            _context = null;
        }

        void PopulateTestData(int blogCount = 25, int postCount = 10)
        {
            for (int i = 0; i < blogCount; i++)
            {
                var blog = new Blog { Name = "Blog" + (i + 1) };

                _context.Blogs.Add(blog);

                for (int j = 0; j < postCount; j++)
                {
                    var post = new Post
                    {
                        Blog = blog,
                        Title = $"Blog {i + 1} - Post {j + 1}",
                        Content = "My Content",
                        PostDate = DateTime.Today.AddDays(-Rnd.Next(0, 100)).AddSeconds(Rnd.Next(0, 30000)),
                        NumberOfReads = Rnd.Next(0, 5000)
                    };

                    _context.Posts.Add(post);
                }
            }

            _context.SaveChanges();
        }

        [Fact]
        public void InterceptWith_TestEqualsToNotEqualsVisitor()
        {
            PopulateTestData(10, 0);

            IQueryable<Blog> query = _context.Blogs.Where(b => b.BlogId % 2 == 0).AsQueryable();

            var visitor = new EqualsToNotEqualsVisitor();
            IQueryable queryIntercepted = query.InterceptWith(visitor);
            Assert.NotNull(queryIntercepted);

            Assert.Equal(typeof(Blog), queryIntercepted.ElementType);
            Assert.Equal("QueryInterceptor.Core.QueryTranslatorProviderAsync", queryIntercepted.Provider.ToString());

            Expression<Func<Blog, bool>> predicate = b => b.BlogId >= 0;
            var queryCreated = queryIntercepted.Provider.CreateQuery(predicate);
            Assert.NotNull(queryCreated);
        }

        [Fact]
        public void Entities_InterceptWith_TestEqualsToNotEqualsVisitor()
        {
            PopulateTestData(10, 0);

            IQueryable<Blog> query = _context.Blogs.Where(b => b.BlogId % 2 == 0).AsQueryable();

            var visitor = new EqualsToNotEqualsVisitor();
            IQueryable<Blog> queryIntercepted = query.InterceptWith(visitor);
            Assert.NotNull(queryIntercepted);

            List<Blog> numbersOdd = query.InterceptWith(visitor).Where(b => b.BlogId >= 0).ToList();
            CollectionAssert.AreEqual(new List<int> { 1, 3, 5, 7, 9 }, numbersOdd.Select(b => b.BlogId).OrderBy(id => id).ToList());
        }
    }
}