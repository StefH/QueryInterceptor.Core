#if EFCORE || EFCORE_6_UP
using System.Collections.Generic;

namespace System.Data.Entity.Infrastructure {
    internal interface IDbAsyncEnumerator<out T> : IAsyncEnumerator<T> {
    }
}
#endif