using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HoaEntities.Entities;
using HoaWebAPI.Extensions.ModelBinders;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.OutputModels;
using HoaInfrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HoaCommon.Extensions.UserClaims;

namespace HoaWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostCollectionsController : ControllerBase
    {
        private IPostRepository _postRepository;
        private IMapper _mapper;
        private IUserRepository _userRepository;

        public PostCollectionsController(IPostRepository postRepository, IMapper mapper,
            IUserRepository userRepository)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        // GET: api/PostCollections
        /// <summary>
        /// Returns a list of Posts
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/PostCollections/e77551ba-78e2-4a36-8754-3ea5f12e1619,e77551ba-78e2-4a36-8754-3ea5f12e1618
        ///
        /// </remarks>
        /// <param name="ids">A list of comma separated GUID</param>
        /// <returns>A list of requested Posts</returns>
        /// <response code="200">Returns a list of Posts</response>
        /// <response code="404">Returns 404 if requested Posts don't exist</response>
        [HttpGet("{ids}", Name = "GetPostCollection")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<IEnumerable<PostViewDto>>> GetPostCollection([ModelBinder(BinderType = typeof(CSVModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var postsFromRepo = await _postRepository.GetPostsAsync(ids);

            if (postsFromRepo?.Count() != ids.Count())
            {
                return BadRequest();
            }

            var postsToReturn = _mapper.Map<IEnumerable<PostViewDto>>(postsFromRepo);

            return Ok(postsToReturn);
        }

        // POST: api/PostCollections
        /// <summary>
        /// Creates a collections of Posts
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/PostCollections
        ///     [{
        ///         "title": "Test1",
        ///         "message": "Test Message",
        ///         "important": "false"
        ///     },
        ///     {
        ///         "title": "TEST2",
        ///         "message": "TEST",
        ///         "important": "true"
        ///     }]
        ///
        /// </remarks>
        /// <param name="posts">Posts to create</param>
        /// <returns>A newly created Posts collection</returns>
        /// <response code="201">Returns the newly created Posts collection</response>
        /// <response code="400">Returns 400 if the input is incorrect</response>
        [HttpPost]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<ActionResult<IEnumerable<PostViewDto>>> CreatePostCollection([FromBody] IEnumerable<PostInputDto> posts)
        {
            if (posts == null)
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

            var postsToAdd = _mapper.Map<IEnumerable<Post>>(posts);

            foreach (var post in postsToAdd)
            {
                post.UserId = user.Id;
                _postRepository.AddPost(post);
            }

            if (!await _postRepository.SaveChangesAsync())
            {
                throw new Exception($"Error adding Events to the database");
            }

            var postsToReturn = _mapper.Map<IEnumerable<PostViewDto>>(postsToAdd);
            var postIds = String.Join(",", postsToReturn.Select(post => post.Id));

            return CreatedAtRoute("GetPostCollection", new { ids = postIds }, postsToReturn);
        }

    }
}