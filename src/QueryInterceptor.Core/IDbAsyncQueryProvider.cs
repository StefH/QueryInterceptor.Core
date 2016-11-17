#if NETSTANDARD
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// Proxy interface to map IDbAsyncQueryProvider (for NET4.5 and dotnet) to IAsyncQueryProvider
    /// </summary>
    public interface IDbAsyncQueryProvider : IAsyncQueryProvider
    {
    }
}
#endif