using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using HoaEntities.Entities;
using HoaCommon.Extensions.UserClaims;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.UpdateModels;
using HoaInfrastructure.Repositories;
using HoaWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace HoaWebAPI.Controllers
{
    [Route("api/posts/{postId}/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private ICommentRepository _commentRepository;
        private IMapper _mapper;
        private IUserRepository _userRepository;
        private IPostRepository _postRepository;
        private ISortFilterService<Comment> _sortFilterService;

        public CommentsController(ICommentRepository commentRepository, IMapper mapper,
            IUserRepository userRepository, IPostRepository postRepository, ISortFilterService<Comment> sortFilterService)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _sortFilterService = sortFilterService;
        }

        // GET: api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/comments
        /// <summary>
        /// Returns a list of Comments. Returned items are paginated with a default page size
        /// of 5 items upto a max of 10 items.
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/comments
        ///     ?sorts=         Message               // sort by title and then by message 
        ///     &#38;filters=   OwnerEmail@=abc,      // filter to Comments that contains the phrase "abc"
        ///     &#38;page=      1                     // get the first page...
        ///     &#38;pageSize=  10                    // ...which contains 10 Comments
        ///
        /// </remarks>
        /// <param name="sieveModel.Filters">See sample request for example of the input parameter</param>
        /// <param name="postId">Id of the post to fetch the comments for</param>
        /// <returns>A list of Comments that are paginated</returns>
        /// <response code="200">Returns a list of Comments</response>
        [HttpGet(Name = "GetComments")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<IEnumerable<CommentViewDto>>> GetCommentsForPost([FromRoute] Guid postId, [FromQuery] SieveModel sieveModel)
        {
            if (!await _postRepository.PostExistsAsync(postId))
            {
                return NotFound();
            }

            var commentsFromRepo = _commentRepository.GetComments(postId);
            var commentsToReturn = _mapper.Map<IEnumerable<CommentViewDto>>(await _sortFilterService.ApplySortsFilters(commentsFromRepo, sieveModel));

            return Ok(commentsToReturn);
        }

        // GET: api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/comments/5
        /// <summary>
        /// Returns a single Comment if it exists or a 404 if it does not
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/comments/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the Comment to return</param>
        /// <param name="postId">Id of the post to get the comment for</param>
        /// <returns>A single Comment</returns>
        /// <response code="200">Returns the Comment requested</response>
        /// <response code="404">Returns a 404 if requested Comment does not exist</response>
        [HttpGet("{id}", Name = "GetComment")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<CommentViewDto>> GetCommentForPost([FromRoute] Guid postId, [FromRoute] Guid id)
        {
            if (!await _postRepository.PostExistsAsync(postId))
            {
                return NotFound();
            }

            var commentFromRepo = await _commentRepository.GetCommentAsync(postId, id);

            if (commentFromRepo == null)
            {
                return NotFound();
            }

            var commentToReturn = _mapper.Map<CommentViewDto>(commentFromRepo);

            return Ok(commentToReturn);
        }

        // POST: api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/Comments
        /// <summary>
        /// Creates a single comment 
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/Comments
        ///     {
        ///        "message": "Test Message",
        ///        "parentId": "e77551ba-78e3-4a36-8754-3ea5f12e1688"
        ///     }
        ///
        /// </remarks>
        /// <param name="commentInputDto">Comment to create</param>
        /// <param name="postId">Id of the post to create the comment for</param>
        /// <returns>A newly created Comment</returns>
        /// <response code="201">Returns the newly created Comment</response>
        /// <response code="400">Returns 400 if the input is incorrect</response>
        [HttpPost]
        [Produces("application/json", "application/xml")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<ActionResult<CommentViewDto>> CreateCommentForPost([FromRoute] Guid postId, [FromBody] CommentInputDto commentInputDto)
        {
            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            if (commentInputDto == null)
            {
                return BadRequest();
            }

            if (!await _postRepository.PostExistsAsync(postId))
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            if (user == null)
            {
                return BadRequest(new { Error = "The user was not found in the system. Please try again with an authorized and valid user." });
            }

            var commentToAdd = _mapper.Map<Comment>(commentInputDto); //map CommentInputDto to Comment
            commentToAdd.UserId = user.Id; //set the user id as otherwise navigation property will be null
            await _commentRepository.AddCommentAsync(postId, commentToAdd);

            if (!await _commentRepository.SaveChangesAsync())
            {
                throw new Exception($"Error saving Comment {commentToAdd.Id} to the database");
            }

            var commentToReturn = _mapper.Map<CommentViewDto>(commentToAdd);

            return CreatedAtRoute("GetComment", new { id = commentToAdd.Id }, commentToReturn);
        }

        // PUT: api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/Comments/5
        /// <summary>
        /// Updates a single comment 
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/Comments/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "message": "Test Message",
        ///        "parentId": "e77551ba-78e3-4a36-8754-3ea5f12e1688"
        ///     }
        ///
        /// </remarks>
        /// <param name="postId">Id of the post to update the comment for</param>
        /// <param name="id">Id of the Comment to update</param>
        /// <param name="commentToUpdate">Comment that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user updating Comment is not the owner of it</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> UpdateCommentForPost([FromRoute] Guid postId, [FromRoute] Guid id,
            [FromBody] CommentUpdateDto commentToUpdate)
        {
            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            if (!await _postRepository.PostExistsAsync(postId))
            {
                return NotFound();
            }

            var commentFromRepo = await _commentRepository.GetCommentAsync(postId, id);

            if (commentFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != commentFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot modify other users submissions unless you are an administrator." });
            }

            _mapper.Map(commentToUpdate, commentFromRepo); //map fields from CommentUpdateDto to Comment that was fetched from repository
            _commentRepository.UpdateComment(commentFromRepo); //Call to empty method. This is just for information and is not required

            if (!await _commentRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating Comment {commentFromRepo.Id} to the database");
            }

            return NoContent();
        }

        // PATCH: api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/Comments/5
        /// <summary>
        /// Updates a single comment with a JSON patch document
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     PATCH api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/Comments/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "op": "replace",
        ///        "path": "/message",
        ///        "value": "Game of thrones message"
        ///     }
        ///
        /// </remarks>
        /// <param name="postId">Id of the post to update the comment for</param>
        /// <param name="id">Id of the Comment to update</param>
        /// <param name="patchDoc">Comment as a patch document that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user deleting Comment is not the owner of it</response>
        [HttpPatch("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> PartialUpdateCommentForPost([FromRoute] Guid postId, [FromRoute] Guid id,
            [FromBody] JsonPatchDocument<CommentUpdateDto> patchDoc)
        {
            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!await _postRepository.PostExistsAsync(postId))
            {
                return NotFound();
            }

            var commentFromRepo = await _commentRepository.GetCommentAsync(postId, id);

            if (commentFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != commentFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot modify other users submissions unless you are an administrator." });
            }

            var commentToPatch = _mapper.Map<CommentUpdateDto>(commentFromRepo); //map Comment to CommentUpdateDto as JsonPatchDocument expects CommentUpdateDto

            patchDoc.ApplyTo(commentToPatch); //apply the update

            TryValidateModel(commentToPatch); //Need to call this as otherwise the patch document will
                                              //be the only thing that is validated for invalid model state
                                              //Need to validate the actual comment model after applying
                                              //the patch document to it to verify for any issues

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState); //need to explicitly call this after TryValidateModel
            }

            _mapper.Map(commentToPatch, commentFromRepo); //map fields from CommentUpdateDto to Comment that was fetched from repository
            _commentRepository.UpdateComment(commentFromRepo); //Call to empty method. This is just for information and is not required

            if (!await _commentRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating Comment {commentFromRepo.Id} to the database");
            }

            return NoContent();
        }

        // DELETE: api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/Comments/5
        /// <summary>
        /// Deletes a single comment
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     DELETE api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/Comments/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="postId">Id of the post to delete the comment for</param>
        /// <param name="id">Id of the Comment to delete</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the Comment was not found</response>
        /// <response code="403">Returns 403 if user updating Comment is not the owner of it</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> DeleteCommentForPost([FromRoute] Guid postId, [FromRoute] Guid id)
        {
            if (!await _postRepository.PostExistsAsync(postId))
            {
                return NotFound();
            }

            var commentFromRepo = await _commentRepository.GetCommentAsync(postId, id);

            if (commentFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != commentFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot delete other users submissions unless you are an administrator." });
            }

            _commentRepository.DeleteComment(commentFromRepo);
            if (!await _commentRepository.SaveChangesAsync())
            {
                throw new Exception($"Error deleting Comment {commentFromRepo.Id} from the database");
            }

            return NoContent();

        }

        // GET: api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/comments/1
        //This action exists to block comment creation by commenting with an Id. This is to adhere to
        //REST practices
        /// <summary>
        /// This action exists to block comment creation by commenting with an Id. This is to adhere to REST practices
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/posts/e77551ba-78e3-4a36-8754-3ea5f12e1619/Comments/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="postId">Id of the post to check for the comment</param>
        /// <param name="id">Id of the Comment</param>
        /// <returns>No Content</returns>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="409">Returns 409 if Comment already exists</response>
        [HttpPost("{id}")]
        public async Task<IActionResult> BlockCommentCreationForPost([FromRoute] Guid postId, Guid id)
        {
            if (!await _postRepository.PostExistsAsync(postId))
            {
                return NotFound();
            }

            if (await _commentRepository.CommentExistsAsync(postId, id))
            {
                return StatusCode(409, new
                {
                    Error = "A Comment with this Id already exists. " +
                     "You cannot create resources by commenting to this endpoint"
                });
            }

            return NotFound();
        }
    }
}