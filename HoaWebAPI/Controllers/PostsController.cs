using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HoaEntities.Entities;
using HoaInfrastructure.Repositories;
using Sieve.Models;
using HoaEntities.Models.OutputModels;
using AutoMapper;
using System;
using HoaEntities.Models.UpdateModels;
using HoaEntities.Models.InputModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using HoaCommon.Extensions.UserClaims;
using HoaWebAPI.Services;

namespace HoaWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private IMapper _mapper;
        private IPostRepository _postRepository;
        private IUserRepository _userRepository;
        private ISortFilterService<Post> _sortFilterService;

        public PostsController(IPostRepository postRepository, IMapper mapper,
            IUserRepository userRepository, ISortFilterService<Post> sortFilterService)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _sortFilterService = sortFilterService;
        }

        // GET: api/Posts
        /// <summary>
        /// Returns a list of Posts. Returned items are paginated with a default page size
        /// of 5 items upto a max of 10 items.
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/Posts
        ///     ?sorts=         Title,Message         // sort by title and then by message 
        ///     &#38;filters=   OwnerEmail@=abc,      // filter to Posts that contains the phrase "abc"
        ///     &#38;page=      1                     // get the first page...
        ///     &#38;pageSize=  10                    // ...which contains 10 Posts
        ///
        /// </remarks>
        /// <param name="sieveModel.Filters">See sample request for example of the input parameter</param>
        /// <returns>A list of Posts that are paginated</returns>
        /// <response code="200">Returns a list of Posts</response>
        [HttpGet(Name = "GetPosts")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<IEnumerable<PostViewDto>>> GetPosts([FromQuery] SieveModel sieveModel)
        {
            var postsFromRepo = _postRepository.GetPosts();
            var postsToReturn = _mapper.Map<IEnumerable<PostViewDto>>(await _sortFilterService.ApplySortsFilters(postsFromRepo, sieveModel));

            return Ok(postsToReturn);
        }

        // GET: api/Posts/5
        /// <summary>
        /// Returns a single Post if it exists or a 404 if it does not
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/Posts/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the Post to return</param>
        /// <returns>A single Post</returns>
        /// <response code="200">Returns the Post requested</response>
        /// <response code="404">Returns a 404 if requested Post does not exist</response>
        [HttpGet("{id}", Name = "GetPost")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<PostViewDto>> GetPost([FromRoute] Guid id)
        {
            var postFromRepo = await _postRepository.GetPostAsync(id);

            if (postFromRepo == null)
            {
                return NotFound();
            }

            var postToReturn = _mapper.Map<PostViewDto>(postFromRepo);

            return Ok(postToReturn);
        }

        //GET: api/Posts/GetLatestPost
        /// <summary>
        /// Returns the latest post
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/Posts/GetLatestPost
        ///
        /// </remarks>
        /// <returns>A single Post</returns>
        /// <response code="200">Returns the latest Post</response>
        [HttpGet("GetLatestPost")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<PostViewDto>> GetLatestPost()
        {
            var postFromRepo = await _postRepository.GetLatestPostAsync();
            var postToReturn = _mapper.Map<PostViewDto>(postFromRepo);

            return Ok(postToReturn);
        }

        // POST: api/Posts
        /// <summary>
        /// Creates a single event 
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/Posts
        ///     {
        ///        "title" :"Test-Title",
	    ///        "message": "Test Message",
	    ///        "important": "false"
        ///     }
        ///
        /// </remarks>
        /// <param name="postInputDto">Post to create</param>
        /// <returns>A newly created Post</returns>
        /// <response code="201">Returns the newly created Post</response>
        /// <response code="400">Returns 400 if the input is incorrect</response>
        [HttpPost]
        [Produces("application/json", "application/xml")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<ActionResult<PostViewDto>> CreatePost([FromBody] PostInputDto postInputDto)
        {
            if (postInputDto == null)
            {
                return BadRequest();
            }

            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            if (user == null)
            {
                return BadRequest(new { Error = "The user was not found in the system. Please try again with an authorized and valid user." });
            }

            var postToAdd = _mapper.Map<Post>(postInputDto); //map PostInputDto to Post
            postToAdd.UserId = user.Id; //set the user id as otherwise navigation property will be null 
            _postRepository.AddPost(postToAdd);

            if (!await _postRepository.SaveChangesAsync())
            {
                throw new Exception($"Error saving Post {postToAdd.Id} to the database");
            }

            var postToReturn = _mapper.Map<PostViewDto>(postToAdd);

            return CreatedAtRoute("GetPost", new { id = postToAdd.Id }, postToReturn);
        }

        // PUT: api/Posts/5
        /// <summary>
        /// Updates a single event 
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     PUT api/Posts/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "title" :"Test-Title",
	    ///        "message": "Test Message",
	    ///        "important": "false"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Id of the Post to update</param>
        /// <param name="postToUpdate">Post that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user updating Post is not the owner of it</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> UpdatePost([FromRoute] Guid id,
            [FromBody] PostUpdateDto postToUpdate)
        {
            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            var postFromRepo = await _postRepository.GetPostAsync(id);

            if (postFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != postFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot modify other users submissions unless you are an administrator." });
            }

            _mapper.Map(postToUpdate, postFromRepo); //map fields from PostUpdateDto to Post that was fetched from repository
            _postRepository.UpdatePost(postFromRepo); //Call to empty method. This is just for information and is not required

            if (!await _postRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating Post {postFromRepo.Id} to the database");
            }

            return NoContent();
        }

        // PATCH: api/Posts/5
        /// <summary>
        /// Updates a single event with a JSON patch document
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     PATCH api/Posts/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "op": "replace",
		///        "path": "/title",
		///        "value": "Game of thrones event"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Id of the Post to update</param>
        /// <param name="patchDoc">Post as a patch document that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user deleting Post is not the owner of it</response>
        [HttpPatch("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> PartialUpdatePost([FromRoute] Guid id,
            [FromBody] JsonPatchDocument<PostUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            var postFromRepo = await _postRepository.GetPostAsync(id);

            if (postFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != postFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot modify other users submissions unless you are an administrator." });
            }

            var postToPatch = _mapper.Map<PostUpdateDto>(postFromRepo); //map Post to PostUpdateDto as JsonPatchDocument expects PostUpdateDto

            patchDoc.ApplyTo(postToPatch); //apply the update

            TryValidateModel(postToPatch); //Need to call this as otherwise the patch document will
                                           //be the only thing that is validated for invalid model state
                                           //Need to validate the actual event model after applying
                                           //the patch document to it to verify for any issues

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState); //need to explicitly call this after TryValidateModel
            }

            _mapper.Map(postToPatch, postFromRepo); //map fields from PostUpdateDto to Post that was fetched from repository
            _postRepository.UpdatePost(postFromRepo); //Call to empty method. This is just for information and is not required

            if (!await _postRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating Post {postFromRepo.Id} to the database");
            }

            return NoContent();
        }

        // DELETE: api/Posts/5
        /// <summary>
        /// Deletes a single event
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     DELETE api/Posts/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the Post to delete</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the Post was not found</response>
        /// <response code="403">Returns 403 if user updating Post is not the owner of it</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> DeletePost([FromRoute] Guid id)
        {
            var postFromRepo = await _postRepository.GetPostAsync(id);

            if (postFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != postFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot delete other users submissions unless you are an administrator." });
            }

            _postRepository.DeletePost(postFromRepo);
            if (!await _postRepository.SaveChangesAsync())
            {
                throw new Exception($"Error deleting Post {postFromRepo.Id} from the database");
            }

            return NoContent();

        }

        //This action exists to block event creation by posting with an Id. This is to adhere to
        //REST practices
        /// <summary>
        /// This action exists to block event creation by posting with an Id. This is to adhere to REST practices
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/Posts/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the Post</param>
        /// <returns>No Content</returns>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="409">Returns 409 if Post already exists</response>
        [HttpPost("{id}")]
        public async Task<IActionResult> BlockPostCreation(Guid id)
        {
            if (await _postRepository.PostExistsAsync(id))
            {
                return StatusCode(409, new
                {
                    Error = "A Post with this Id already exists. " +
                     "You cannot create resources by posting to this endpoint"
                });
            }

            return NotFound();
        }
    }
}