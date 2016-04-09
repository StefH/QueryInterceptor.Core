#if DNXCORE50
using Microsoft.Data.Entity.Query.Internal;

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// Proxy interface to map IDbAsyncQueryProvider (for NET4.5 and dnxcore50) to IAsyncQueryProvider (used in dnxcore50)
    /// </summary>
    public interface IDbAsyncQueryProvider : IAsyncQueryProvider
    {
    }
}
#endif