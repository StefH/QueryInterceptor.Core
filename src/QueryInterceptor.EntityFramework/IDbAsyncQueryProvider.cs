#if EFCORE
using Microsoft.EntityFrameworkCore.Query.Internal;
#elif EFCORE7
using Microsoft.EntityFrameworkCore.Query;
#endif

#if EFCORE || EFCORE7
namespace System.Data.Entity.Infrastructure
{
    internal interface IDbAsyncQueryProvider : IAsyncQueryProvider
    {
    }
}
#endif

/*
 * #if EFCORE
using Microsoft.EntityFrameworkCore.Query.Internal;
#elif EFCORE7
using Microsoft.EntityFrameworkCore.Query;
#endif

#if EFCORE || EFCORE7
namespace System.Data.Entity.Infrastructure {
    internal interface IDbAsyncQueryProvider : IAsyncQueryProvider {

    }
}
#endif*/