using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoaInfrastructure.Context;
using HoaEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoaInfrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private HoaDbContext _context;

        public CommentRepository(HoaDbContext hoaDbContextbContext)
        {
            _context = hoaDbContextbContext ?? throw new ArgumentNullException(nameof(hoaDbContextbContext));
        }

        public async Task AddCommentAsync(Guid postId, Comment commentToAdd)
        {
            if (commentToAdd == null)
            {
                throw new ArgumentNullException(nameof(commentToAdd));
            }

            //we are adding a child entity here so we need to fetch the post entity first
            //note that no null check if done here because the controller will take care of checking
            //if the post exists or not. The repository is purely for adding/updating/deleting
            var postFromRepo = await _context.Posts.Include(post => post.Comments).FirstOrDefaultAsync(post => post.Id == postId);

            //add the comment to the post that was fetched
            postFromRepo.Comments.Add(commentToAdd);
        }

        public async Task<bool> CommentExistsAsync(Guid postId, Guid commentId)
        {
            return await _context.Comments.AnyAsync(comment => comment.PostId == postId && comment.Id == commentId);
        }

        public void DeleteComment(Comment commentToDelete)
        {
            if (commentToDelete == null)
            {
                throw new ArgumentNullException(nameof(commentToDelete));
            }

            //for comments, to preserve the structure of the page, deleted comments will have
            //their contents replace with below message. This way, the page can still retain
            //what the parent comment/etc is
            var comment = _context.Comments.FirstOrDefault(x => x.Id == commentToDelete.Id);
            comment.Message = "[Deleted]";
            //_context.Comments.Remove(commentToDelete);
        }

        public async Task<Comment> GetCommentAsync(Guid postId, Guid commentId)
        {
            //fetch the comment where the postid and commentid matches
            //need to ensure that the comment's postid is similar 
            return await _context.Comments.Include(comment => comment.CreatedBy)
                           .Where(comment => comment.Id == commentId && comment.PostId == postId)
                           .FirstOrDefaultAsync();
        }

        public IQueryable<Comment> GetComments(Guid postId)
        {
            return _context.Comments.Include(comment => comment.CreatedBy)
                                    .Where(comment => comment.PostId == postId)
                                    .OrderByDescending(comment => comment.Created)
                                    .AsNoTracking();
        }

        //public async Task<IEnumerable<Comment>> GetCommentsAsync(Guid postId, SieveModel sieveModel)
        //{
        //    //get max and default page values
        //    int maxPageSize = Int32.Parse(_configuration["Sieve:MaxPageSize"]);
        //    int defaultPageSize = Int32.Parse(_configuration["Sieve:DefaultPageSize"]);

        //    //set page size and number and check for null values as the library does not support default values
        //    int pageSize = sieveModel.PageSize ?? defaultPageSize;
        //    int pageNumber = sieveModel.Page ?? 1;
        //    pageSize = pageSize > maxPageSize ? defaultPageSize : pageSize;

        //    //fetch the comments from the repo
        //    //filter the comments only for the specific postid
        //    var comments = _context.Comments.Include(comment => comment.CreatedBy)
        //                           .Where(comment => comment.PostId == postId)
        //                           .AsNoTracking();

        //    //apply sorts and filters prior to getting total count for generating headers
        //    //this is so that headers don't return incorrect values due to filters applied
        //    //note that filters are applied first prior to sorts with Sieve.Apply
        //    comments = _sieveProcessor.Apply(sieveModel, comments, applyPagination: false);
        //    _paginationGenerator.GenerateHeaders(comments.Count(), pageSize, pageNumber, sieveModel.Sorts, sieveModel.Filters);

        //    //apply the pagination and don't re-apply filtering and sorting as they have already been completed
        //    var commentsToReturn = _sieveProcessor.Apply(sieveModel, comments, applyFiltering: false, applySorting: false);

        //    return await commentsToReturn.ToListAsync();
        //}

        public async Task<IEnumerable<Comment>> GetCommentsAsync(Guid postId, IEnumerable<Guid> ids)
        {
            //fetch the comments where the postid and commentids match
            //need to ensure that the comments postid is similar 
            return await _context.Comments.Include(comment => comment.CreatedBy)
                                 .Where(comment => comment.PostId == postId && ids.Contains(comment.Id))
                                 .ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            // return true if 1 or more entities were changed
            return (await _context.SaveChangesAsync() >= 0);
        }

        public void UpdateComment(Comment commentToUpdate)
        {
            //nothing required
            //Mapping CommentUpdateDto to Comment automatically makes the EF context track the object
            //The Comment will start being tracked and can be updated
        }
    }
}
