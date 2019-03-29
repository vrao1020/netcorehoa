using HoaWebApplication.Models.CacheModel;
using System;
using System.Threading.Tasks;

namespace HoaWebApplication.Services.Cache
{
    public interface ICacheStore
    {
        Task<CacheDto> GetCacheValueAsync(string key);
        Task<(DateTime date, string access_token)> GetExpirationAccessTokenAsync(string key);
        Task SetCacheValueAsync(string key, string etag, string response, string pagingHeader);
        Task SetCacheValueAsync(string key, DateTime value, string access_token);
        Task DeleteCacheValueAsync(string key);
    }
}
