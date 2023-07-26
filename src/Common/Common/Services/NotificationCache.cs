using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace AllSub.Common.Services
{
    // TODO: Replace in memory cache with distributed cache on production
    public class NotificationCache : INotificationCache
    {
        private readonly IMemoryCache _memoryCache;

        public NotificationCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void ClearData(string connectionId)
        {
            _memoryCache.Remove(CreateCacheKey(connectionId));
        }

        public IDictionary<string, string>? GetData(string connectionId)
        {
            return _memoryCache.Get<IDictionary<string, string>>(CreateCacheKey(connectionId));
        }

        public void SetData(string connectionId, IDictionary<string, string> dataDict)
        {
            _memoryCache.Set(CreateCacheKey(connectionId), dataDict, TimeSpan.FromHours(2));
        }

        private static string CreateCacheKey(string connectionId)
        {
            return  $"Services.NotificationCache.{connectionId}";
        }
    }
}
