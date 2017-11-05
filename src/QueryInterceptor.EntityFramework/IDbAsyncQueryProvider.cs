#if EFCORE
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace System.Data.Entity.Infrastructure
{
    internal interface IDbAsyncQueryProvider : IAsyncQueryProvider
    {
    }
}
#endif