#if EF || EFCORE
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace QueryInterceptor.Core
{
    public class QueryTranslatorDbAsyncEnumerator<T> : IDbAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public QueryTranslatorDbAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        // System.Data.Entity.Infrastructure.IDbAsyncEnumerator
        public Task<bool> MoveNextAsync(CancellationToken cancellationToken) => Task.FromResult(_inner.MoveNext());

        // System.Collections.Generic.IAsyncEnumerator
        public Task<bool> MoveNext(CancellationToken cancellationToken) => Task.FromResult(_inner.MoveNext());

        public T Current => _inner.Current;

#if EF
        object IDbAsyncEnumerator.Current => Current;
#endif
    }
}
#endif