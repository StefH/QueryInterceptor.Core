using System.Data.Entity.Infrastructure;

namespace QueryInterceptor.Core;

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

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public T Current => _inner.Current;
}