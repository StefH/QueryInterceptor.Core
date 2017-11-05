using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QueryInterceptor.Core.ExpressionVisitors;
using QueryInterceptor.UnitTests.Helpers.Entities;
using Xunit;
using QueryInterceptor.Core;

#if EFCORE
namespace QueryInterceptor.EntityFrameworkCore
#else
namespace QueryInterceptor.Core.UnitTests
#endif
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
            builder.UseSqlite($"Filename=QueryInterceptor.Core.DB.{Guid.NewGuid()}.db");
            //builder.UseInMemoryDatabase();

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

        // Fixed : EF issue https://github.com/aspnet/EntityFramework/issues/4968
        [Fact]
        public void Entities_Select_BlogAndPosts()
        {
            //Arrange
            PopulateTestData(5, 5);

            //Act
            var expected = _context.Blogs.Select(x => new { x.BlogId, x.Name, x.Posts }).ToArray();

            //Assert
            Assert.Equal(5, expected.Length);
        }

        [Fact]
        public void Entities_InterceptWith_IQueryable()
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
        public void Entities_InterceptWith_EqualsToNotEqualsVisitor()
        {
            PopulateTestData(10, 0);

            IQueryable<Blog> query = _context.Blogs.Where(b => b.BlogId % 2 == 0).AsQueryable();

            var visitor = new EqualsToNotEqualsVisitor();
            IQueryable<Blog> queryIntercepted = query.InterceptWith(visitor);
            Assert.NotNull(queryIntercepted);

            Assert.Equal(new List<int> { 1, 3, 5, 7, 9 }, queryIntercepted.Select(b => b.BlogId).OrderBy(id => id).ToList());
        }

        [Fact]
        public void Entities_Select()
        {
            _context.Blogs.Add(new Blog { Name = "A" });
            _context.Blogs.Add(new Blog { Name = "a" });
            _context.SaveChanges();

            IQueryable<Blog> query = _context.Blogs.AsQueryable();

            List<Blog> queryIntercepted1 = query.Where(b => b.Name == "A").ToList();
            Assert.Equal(new List<string> { "A" }, queryIntercepted1.Select(b => b.Name).ToList());

            List<Blog> queryIntercepted2 = query.Where(b => b.Name == "a").ToList();
            Assert.Equal(new List<string> { "a" }, queryIntercepted2.Select(b => b.Name).OrderBy(x => x).ToList());
        }

        [Fact]
        public void Entities_InterceptWith_StringComparisonVisitor_CurrentCultureIgnoreCase()
        {
            _context.Blogs.Add(new Blog { Name = "A" });
            _context.Blogs.Add(new Blog { Name = "a" });
            _context.SaveChanges();

            IQueryable<Blog> query = _context.Blogs.AsQueryable();

            var visitor = new StringComparisonVisitor(StringComparison.CurrentCultureIgnoreCase);

            List<Blog> queryIntercepted1 = query.InterceptWith(visitor).Where(b => b.Name == "A").ToList();
            Assert.Equal(new List<string> { "a", "A" }, queryIntercepted1.Select(b => b.Name).OrderBy(x => x).ToList());

            List<Blog> queryIntercepted2 = query.InterceptWith(visitor).Where(b => b.Name == "a").ToList();
            Assert.Equal(new List<string> { "a", "A" }, queryIntercepted2.Select(b => b.Name).OrderBy(x => x).ToList());
        }

        [Fact]
        public void Entities_InterceptWith_StringComparisonVisitor_CurrentCulture()
        {
            _context.Blogs.Add(new Blog { Name = "A" });
            _context.Blogs.Add(new Blog { Name = "a" });
            _context.SaveChanges();

            IQueryable<Blog> query = _context.Blogs.AsQueryable();

            var visitor = new StringComparisonVisitor(StringComparison.CurrentCulture);

            List<Blog> queryIntercepted1 = query.InterceptWith(visitor).Where(b => b.Name == "A").ToList();
            Assert.Equal(new List<string> { "A" }, queryIntercepted1.Select(b => b.Name).ToList());

            List<Blog> queryIntercepted2 = query.InterceptWith(visitor).Where(b => b.Name == "a").ToList();
            Assert.Equal(new List<string> { "a" }, queryIntercepted2.Select(b => b.Name).OrderBy(x => x).ToList());
        }
    }
}