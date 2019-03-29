using Sieve.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoaWebAPI.Services
{
    public interface ISortFilterService<T>
    {
        Task<IEnumerable<T>> ApplySortsFilters(IQueryable<T> collectionToSortFilter, SieveModel sieveModel);
    }
}
