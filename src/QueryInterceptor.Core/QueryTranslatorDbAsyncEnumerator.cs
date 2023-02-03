#if EF || EFCORE
using System.Data.Entity.Infrastructure;

namespace QueryInterceptor.Core {
    public class QueryTranslatorDbAsyncEnumerator<T> : IDbAsyncEnumerator<T> {
        public T Current => _inner.Current;

        readonly IEnumerator<T> _inner;

        public QueryTranslatorDbAsyncEnumerator(IEnumerator<T> inner) {
            _inner = inner;
        }

        public ValueTask DisposeAsync() {
            Dispose();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        public void Dispose() {
            _inner.Dispose();
        }

        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    }
}
#endif