#if EFCORE
using Microsoft.EntityFrameworkCore.Query.Internal;
#elif EFCORE_6_UP
using Microsoft.EntityFrameworkCore.Query;
#endif

#if EFCORE || EFCORE_6_UP
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
#elif EFCORE_6_UP
using Microsoft.EntityFrameworkCore.Query;
#endif

#if EFCORE || EFCORE_6_UP
namespace System.Data.Entity.Infrastructure {
    internal interface IDbAsyncQueryProvider : IAsyncQueryProvider {

    }
}
#endif*/