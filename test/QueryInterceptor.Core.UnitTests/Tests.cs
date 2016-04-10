using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using QueryInterceptor.Core.ExpressionVisitors;
using Xunit;
#if DNXCORE50
using Microsoft.Data.Entity;
#else
using System.Data.Entity;
#endif

namespace QueryInterceptor.Core.UnitTests
{
    public class Tests
    {
        [Fact]
        public void InterceptWith_IQueryable()
        {
            IQueryable<int> query = Enumerable.Range(0, 10).AsQueryable().Where(n => n % 2 == 0);

            var visitor = new EqualsToNotEqualsVisitor();
            IQueryable queryIntercepted = query.InterceptWith(visitor);
            Assert.NotNull(queryIntercepted);

            Assert.Equal(typeof(int), queryIntercepted.ElementType);
            Assert.Equal("QueryInterceptor.Core.QueryTranslatorProviderAsync", queryIntercepted.Provider.ToString());

            Expression<Func<int, bool>> predicate = x => x >= 0;
            var queryCreated = queryIntercepted.Provider.CreateQuery(predicate);
            Assert.NotNull(queryCreated);
        }

        [Fact]
        public void InterceptWith_TestEqualsToNotEqualsVisitor()
        {
            IQueryable<int> query = Enumerable.Range(0, 10).AsQueryable().Where(n => n % 2 == 0);
            List<int> numbersEven = query.ToList();
            Assert.Equal(new List<int> { 0, 2, 4, 6, 8 }, numbersEven);

            var visitor = new EqualsToNotEqualsVisitor();
            List<int> numbersOdd = query.InterceptWith(visitor).Where(x => x >= 0).ToList();
            Assert.Equal(new List<int> { 1, 3, 5, 7, 9 }, numbersOdd);
        }

        [Fact]
        public void InterceptWith_TestEqualsToNotEqualsVisitor_FirstAsync()
        {
            var queryEven = Enumerable.Range(0, 10).AsQueryable().Where(n => n % 2 == 0).AsQueryable();

            var visitor = new EqualsToNotEqualsVisitor();
            var queryOdd = queryEven.InterceptWith(visitor);

            var task = queryOdd.FirstAsync(n => n > 5, CancellationToken.None);
            task.ContinueWith(t => t.Result).Wait();

            Assert.Equal(7, task.Result);
        }

        [Fact]
        public void InterceptWith_TestSetComparerExpressionVisitor_CurrentCultureIgnoreCase()
        {
            IQueryable<string> query = new List<string> { "A", "a" }.AsQueryable();

            var visitor = new StringComparisonVisitor(StringComparison.CurrentCultureIgnoreCase);

            List<string> queryIntercepted1 = query.InterceptWith(visitor).Where(s => s == "A").ToList();
            Assert.Equal(new List<string> { "A", "a" }, queryIntercepted1);

            List<string> queryIntercepted2 = query.InterceptWith(visitor).Where(s => s == "a").ToList();
            Assert.Equal(new List<string> { "A", "a" }, queryIntercepted2);
        }

        [Fact]
        public void InterceptWith_TestSetComparerExpressionVisitor_CurrentCulture()
        {
            IQueryable<string> query = new List<string> { "A", "a" }.AsQueryable();

            var visitor = new StringComparisonVisitor(StringComparison.CurrentCulture);

            List<string> queryIntercepted1 = query.InterceptWith(visitor).Where(s => s == "A").ToList();
            Assert.Equal(new List<string> { "A" }, queryIntercepted1);

            List<string> queryIntercepted2 = query.InterceptWith(visitor).Where(s => s == "a").ToList();
            Assert.Equal(new List<string> { "a" }, queryIntercepted2);
        }
    }
}