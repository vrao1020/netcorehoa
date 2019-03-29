using HoaEntities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoaInfrastructure.Repositories
{
    public interface IPostRepository
    {
        //Task<IEnumerable<Post>> GetPostsAsync(SieveModel sieveModel);
        IQueryable<Post> GetPosts();
        Task<Post> GetPostAsync(Guid postId);
        Task<Post> GetLatestPostAsync();
        void AddPost(Post postToAdd);//This not asynchronous because the object is added to the db context for tracking but its not saved yet.
                                     //So there is no need to make this async as saving the object will take care of the async operation
        void DeletePost(Post postToDelete);
        Task<bool> SaveChangesAsync();
        Task<bool> PostExistsAsync(Guid postId);
        void UpdatePost(Post postToUpdate); //This is only for information purpose. This will be an empty implementation
        Task<IEnumerable<Post>> GetPostsAsync(IEnumerable<Guid> ids);
    }
}
