using HoaEntities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoaInfrastructure.Repositories
{
    public interface ICommentRepository
    {
        //Task<IEnumerable<Comment>> GetCommentsAsync(Guid postId, SieveModel sieveModel);
        IQueryable<Comment> GetComments(Guid postId);
        Task<Comment> GetCommentAsync(Guid postId, Guid commentId);
        Task AddCommentAsync(Guid postId, Comment commentToAdd);//This not asynchronous because the object is added to the db context for tracking but its not saved yet.
                                                                //So there is no need to make this async as saving the object will take care of the async operation
        void DeleteComment(Comment commentToDelete);
        Task<bool> SaveChangesAsync();
        Task<bool> CommentExistsAsync(Guid postId, Guid commentId);
        void UpdateComment(Comment commentToUpdate); //This is only for information purpose. This will be an empty implementation
        Task<IEnumerable<Comment>> GetCommentsAsync(Guid postId, IEnumerable<Guid> ids);
    }
}
