﻿#if EFCORE || EFCORE7
using System.Collections.Generic;

namespace System.Data.Entity.Infrastructure {
    internal interface IDbAsyncEnumerator<out T> : IAsyncEnumerator<T> {
    }
}
#endif