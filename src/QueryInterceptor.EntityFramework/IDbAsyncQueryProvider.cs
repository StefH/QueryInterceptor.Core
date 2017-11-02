#if EFCORE
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// Proxy interface to map IDbAsyncQueryProvider (for NET 4.5 and dotnet) to IAsyncQueryProvider
    /// </summary>
    internal interface IDbAsyncQueryProvider : IAsyncQueryProvider
    {
    }
}
#endif