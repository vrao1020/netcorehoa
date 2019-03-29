using System;
using System.Threading.Tasks;
using HoaWebApplication.Models.CacheModel;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace HoaWebApplication.Services.Cache
{
    public class CacheStore : ICacheStore
    {
        private IDistributedCache _cache;

        public CacheStore(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task DeleteCacheValueAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task<(DateTime date, string access_token)> GetExpirationAccessTokenAsync(string key)
        {
            var cachedResults = await _cache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cachedResults))
            {
                //create an anonymous object that will hold the values fetches by the cache
                var anonymousObject = new { date = "", access_token = "" };

                //deserialize the cached result into the anonymous object
                var deserializedObject = JsonConvert.DeserializeAnonymousType(cachedResults, anonymousObject);

                //return the date but check to see if date is null and provide current time
                //otherwise will throw an exception
                return (DateTime.Parse(deserializedObject.date), deserializedObject.access_token);
            }

            return (DateTime.Now, null);
        }

        public async Task<CacheDto> GetCacheValueAsync(string key)
        {
            var cachedResults = await _cache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cachedResults))
            {
                var result = JsonConvert.DeserializeObject<CacheDto>(cachedResults);
                return result;
            }
            else
            {
                return null;
            }
        }

        public async Task SetCacheValueAsync(string key, string etag, string response, string pagingHeader)
        {
            CacheDto cacheDto = new CacheDto(etag, response, pagingHeader);

            string cacheItem = JsonConvert.SerializeObject(cacheDto, new JsonSerializerSettings()
            {
                //set reference loop handling to ignore otherwise navigation properties will cause problems
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            await _cache.SetStringAsync(key, cacheItem);
        }

        public async Task SetCacheValueAsync(string key, DateTime value, string access_token)
        {
            string cacheItem = JsonConvert.SerializeObject(new
            {
                date = value.ToString(),
                access_token = access_token
            });

            await _cache.SetStringAsync(key, cacheItem);
        }
    }
}
