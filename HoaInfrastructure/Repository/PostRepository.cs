using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoaInfrastructure.Context;
using HoaEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoaInfrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private HoaDbContext _context;

        public PostRepository(HoaDbContext hoaDbContextbContext)
        {
            _context = hoaDbContextbContext ?? throw new ArgumentNullException(nameof(hoaDbContextbContext));
        }

        public void AddPost(Post postToAdd)
        {
            if (postToAdd == null)
            {
                throw new ArgumentNullException(nameof(postToAdd));
            }

            _context.Posts.Add(postToAdd);
        }

        public void DeletePost(Post postToDelete)
        {
            if (postToDelete == null)
            {
                throw new ArgumentNullException(nameof(postToDelete));
            }

            _context.Posts.Remove(postToDelete);
        }

        public Task<Post> GetLatestPostAsync()
        {
            return _context.Posts.Include(s => s.CreatedBy)
                           .OrderByDescending(x => x.Created)
                           .FirstOrDefaultAsync();
        }

        public async Task<Post> GetPostAsync(Guid postId)
        {
            return await _context.Posts.Include(post => post.CreatedBy)
                                 .FirstOrDefaultAsync(post => post.Id == postId);
        }

        public IQueryable<Post> GetPosts()
        {
            return _context.Posts.Include(post => post.CreatedBy)
                                 .Include(post => post.Comments)
                                 .OrderByDescending(post => post.Created)
                                 .AsNoTracking();
        }

        //public async Task<IEnumerable<Post>> GetPostsAsync(SieveModel sieveModel)
        //{
        //    //get max and default page values
        //    int maxPageSize = Int32.Parse(_configuration["Sieve:MaxPageSize"]);
        //    int defaultPageSize = Int32.Parse(_configuration["Sieve:DefaultPageSize"]);

        //    //set page size and number and check for null values as the library does not support default values
        //    int pageSize = sieveModel.PageSize ?? defaultPageSize;
        //    int pageNumber = sieveModel.Page ?? 1;
        //    pageSize = pageSize > maxPageSize ? defaultPageSize : pageSize;

        //    //fetch the events from the repo
        //    var posts = _context.Posts.Include(post => post.CreatedBy)
        //                              .Include(post => post.Comments)
        //                              .AsNoTracking();

        //    //apply sorts and filters prior to getting total count for generating headers
        //    //this is so that headers don't return incorrect values due to filters applied
        //    //note that filters are applied first prior to sorts with Sieve.Apply
        //    posts = _sieveProcessor.Apply(sieveModel, posts, applyPagination: false);
        //    _paginationGenerator.GenerateHeaders(posts.Count(), pageSize, pageNumber, sieveModel.Sorts, sieveModel.Filters);

        //    //apply the pagination and don't re-apply filtering and sorting as they have already been completed
        //    var postsToReturn = _sieveProcessor.Apply(sieveModel, posts, applyFiltering: false, applySorting: false);

        //    return await postsToReturn.ToListAsync();
        //}

        public async Task<IEnumerable<Post>> GetPostsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Posts.Include(post => post.CreatedBy)
                                 .Include(post => post.Comments)
                                 .Where(post => ids.Contains(post.Id))
                                 .ToListAsync();
        }

        public async Task<bool> PostExistsAsync(Guid postId)
        {
            return await _context.Posts.AnyAsync(post => post.Id == postId);
        }

        public async Task<bool> SaveChangesAsync()
        {
            // return true if 1 or more entities were changed
            return (await _context.SaveChangesAsync() >= 0);
        }

        public void UpdatePost(Post postToUpdate)
        {
            //nothing required
            //Mapping PostUpdateDto to Post automatically makes the EF context track the object
            //The Post will start being tracked and can be updated
        }
    }
}
