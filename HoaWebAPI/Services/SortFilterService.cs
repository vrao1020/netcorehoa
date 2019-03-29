using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoaCommon.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sieve.Models;
using Sieve.Services;

namespace HoaWebAPI.Services
{
    public class SortFilterService<T> : ISortFilterService<T> where T : class
    {
        private ISieveProcessor _sieveProcessor;
        private IPaginationGenerator _paginationGenerator;
        private IConfiguration _configuration;

        public SortFilterService(ISieveProcessor sieveProcessor,
            IPaginationGenerator paginationGenerator, IConfiguration configuration)
        {
            _sieveProcessor = sieveProcessor;
            _paginationGenerator = paginationGenerator;
            _configuration = configuration;
        }

        public async Task<IEnumerable<T>> ApplySortsFilters(IQueryable<T> collectionToSortFilter, SieveModel sieveModel)
        {
            //get max and default page values
            var maxPageSize = Int32.Parse(_configuration["Sieve:MaxPageSize"]);
            var defaultPageSize = Int32.Parse(_configuration["Sieve:DefaultPageSize"]);

            //set page size and number and check for null values as the library does not support default values
            var pageSize = sieveModel.PageSize ?? defaultPageSize;
            var pageNumber = sieveModel.Page ?? 1;
            pageSize = pageSize > maxPageSize ? defaultPageSize : pageSize;

            //apply sorts and filters prior to getting total count for generating headers
            //this is so that headers don't return incorrect values due to filters applied
            //note that filters are applied first prior to sorts with Sieve.Apply
            collectionToSortFilter = _sieveProcessor.Apply(sieveModel, collectionToSortFilter, applyPagination: false);
            _paginationGenerator.GenerateHeaders(collectionToSortFilter.Count(), pageSize, pageNumber, sieveModel.Sorts, sieveModel.Filters);

            //apply the pagination and don't re-apply filtering and sorting as they have already been completed
            var collectionToReturn = _sieveProcessor.Apply(sieveModel, collectionToSortFilter, applyFiltering: false, applySorting: false);

            return await collectionToReturn.ToListAsync();
        }
    }
}
