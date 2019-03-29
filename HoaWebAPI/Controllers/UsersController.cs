using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HoaEntities.Entities;
using AutoMapper;
using HoaInfrastructure.Repositories;
using Sieve.Models;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.UpdateModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using HoaCommon.Extensions.UserClaims;
using Marvin.Cache.Headers;
using HoaWebAPI.Services;

namespace HoaWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private IMapper _mapper;
        private IUserRepository _userRepository;
        private ISortFilterService<User> _sortFilterService;

        public UsersController(IMapper mapper, IUserRepository userRepository, ISortFilterService<User> sortFilterService)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _sortFilterService = sortFilterService;
        }

        //GET: api/Users/CurrentUser
        /// <summary>
        /// Returns the current User that is logged into the web application
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/Users/CurrentUser
        ///     
        /// </remarks>
        /// <returns>The current User that is logged into the web application</returns>
        /// <response code="200">The current User that is logged into the web application</response>
        /// <response code="400">Returns 400 is user is not found</response>
        [HttpGet("CurrentUser", Name = "GetCurrentUser")]
        [Produces("application/json", "application/xml")]
        [HttpCacheExpiration(NoStore = true)]
        public async Task<ActionResult<UserViewDto>> GetCurrentUser()
        {
            var userFromRepo = await _userRepository.GetUserAsync(User.GetUserId());

            if (userFromRepo == null)
            {
                return NotFound();
            }

            var userToReturn = _mapper.Map<UserViewDto>(userFromRepo);

            return Ok(userToReturn);
        }

        // GET: api/Users
        /// <summary>
        /// Returns a list of Users. Returned items are paginated with a default page size
        /// of 5 items upto a max of 10 items. Only accessible by Administrators
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/Users
        ///     ?sorts=         FirstName,LastName    // sort by FirstName and then by LastName 
        ///     &#38;filters=   Email@=abc,           // filter to Users that contains the phrase "abc"
        ///     &#38;page=      1                     // get the first page...
        ///     &#38;pageSize=  10                    // ...which contains 10 Users
        ///
        /// </remarks>
        /// <param name="sieveModel.Filters">See sample request for example of the input parameter</param>
        /// <returns>A list of Users that are paginated</returns>
        /// <response code="200">Returns a list of Users</response>
        [HttpGet(Name = "GetAllUsers")]
        [Authorize(Policy = "AdminUser")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<IEnumerable<UserViewDto>>> GetUsers([FromQuery] SieveModel sieveModel)
        {
            var users = _userRepository.GetUsers();
            var usersToReturn = _mapper.Map<IEnumerable<UserViewDto>>(await _sortFilterService.ApplySortsFilters(users, sieveModel));

            return Ok(usersToReturn);
        }

        //GET: api/Users/{guid}
        /// <summary>
        /// Returns a single User if it exists or a 404 if it does not. Only accessible by Administrators
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/Users/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the User to return</param>
        /// <returns>A single User</returns>
        /// <response code="200">Returns the User requested</response>
        /// <response code="404">Returns a 404 if requested User does not exist</response>
        [HttpGet("{id}", Name = "GetUser")]
        [Produces("application/json", "application/xml")]
        [Authorize(Policy = "AdminUser")]
        public async Task<ActionResult<UserViewDto>> GetUser([FromRoute] Guid id)
        {
            var userFromRepo = await _userRepository.GetUserAsync(id);

            if (userFromRepo == null)
            {
                return NotFound();
            }

            var userToReturn = _mapper.Map<UserViewDto>(userFromRepo);

            return Ok(userToReturn);
        }

        //GET: api/Users/ReminderEmails
        /// <summary>
        /// Returns a list of user emails who have opted to receive reminders
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/Users/ReminderEmails
        ///
        /// </remarks>
        /// <returns>A single User</returns>
        /// <response code="200">Returns the emails of all users who opted to receive reminders</response>
        [HttpGet("ReminderEmails")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<IEnumerable<string>>> GetReminderEmails()
        {
            var userEmails = await _userRepository.GetUsersWithRemindersAsync();

            return Ok(userEmails);
        }

        // POST: api/Users
        /// <summary>
        /// Creates a single User 
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/Users
        ///     {
        ///        "firstname" :"Test-Title",
	    ///        "lastname": "Test Message",
	    ///        "reminder": "false",
	    ///        "email": "abc@gmail.com"
        ///     }
        ///
        /// </remarks>
        /// <param name="userInputDto">User to create</param>
        /// <returns>A newly created User</returns>
        /// <response code="201">Returns the newly created User</response>
        /// <response code="400">Returns 400 if the input is incorrect</response>
        [HttpPost]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<UserViewDto>> CreateUser([FromBody] UserInputDto userInputDto)
        {
            if (userInputDto == null)
            {
                return BadRequest();
            }

            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            if (user != null)
            {
                return new ConflictObjectResult(new { Error = "The user already exists in the system. You cannot add the same user again." });
            }

            var userToAdd = _mapper.Map<User>(userInputDto); //map UserInputDto to User 
            userToAdd.SocialId = User.GetUserId(); //map social Id as it will be null otherwise
            _userRepository.AddUser(userToAdd);

            if (!await _userRepository.SaveChangesAsync())
            {
                throw new Exception($"Error saving User {userToAdd.Id} to the database");
            }

            var userToReturn = _mapper.Map<UserViewDto>(userToAdd);

            return CreatedAtRoute("GetUser", new { id = userToAdd.Id }, userToReturn);
        }

        // POST: api/Users/DeactiveUser
        /// <summary>
        /// Deactivates the current user logged into the web application
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/Users/DeactiveUser
        ///
        /// </remarks>
        /// <returns>Deactivates the current logged in user</returns>
        /// <response code="204">No content if successful</response>
        /// <response code="400">Returns 400 if the input is incorrect</response>
        [HttpPost("DeactivateUser")]
        public async Task<IActionResult> DeactivateUser()
        {
            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            if (user == null)
            {
                return BadRequest(new { Error = "The user was not found in the system. Please try again with an authorized and valid user." });
            }

            user.Active = false;

            if (!await _userRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating User to the database");
            }

            return NoContent();
        }

        // POST: api/Users/ActivateUser
        /// <summary>
        /// Activates the current user logged into the web application
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/Users/ActivateUser
        ///
        /// </remarks>
        /// <returns>Activates the current logged in user</returns>
        /// <response code="204">No content if successful</response>
        /// <response code="400">Returns 400 if the input is incorrect</response>
        [HttpPost("ActivateUser")]
        public async Task<IActionResult> ActivateUser()
        {
            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            if (user == null)
            {
                return BadRequest(new { Error = "The user was not found in the system. Please try again with an authorized and valid user." });
            }

            user.Active = true;

            if (!await _userRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating User to the database");
            }

            return NoContent();
        }

        // PUT: api/Users
        /// <summary>
        /// Updates the currently logged in User 
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     PUT api/Users/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "firstname" :"Test-Title",
	    ///        "lastname": "Test Message",
	    ///        "reminder": "false",
	    ///        "email": "abc@gmail.com"
        ///     }
        ///
        /// </remarks>
        /// <param name="userToUpdate">User that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user updating User is not the owner of it</response>
        [HttpPut]
        [ProducesResponseType(201)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto userToUpdate)
        {
            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var userFromRepo = await _userRepository.GetUserAsync(User.GetUserId());

            //upsert with PUT and create a new user
            //this is for creating/updating user accounts whenever a user logs into the web app
            if (userFromRepo == null)
            {
                var userToAdd = _mapper.Map<User>(userToUpdate); //map UserUpdateDto to User 
                userToAdd.SocialId = User.GetUserId(); //map social Id as it will be null otherwise
                _userRepository.AddUser(userToAdd);

                if (!await _userRepository.SaveChangesAsync())
                {
                    throw new Exception($"Error saving User {userToAdd.Id} to the database");
                }

                var userToReturn = _mapper.Map<UserViewDto>(userToAdd);

                return CreatedAtRoute("GetUser", new { id = userToAdd.Id }, userToReturn);
            }

            //don't need to check for ownership as this will only allow the current authorized user to modify their own data

            _mapper.Map(userToUpdate, userFromRepo); //map fields from UserUpdateDto to User that was fetched from repository
            _userRepository.UpdateUser(userFromRepo); //Call to empty method. This is just for information and is not required

            if (!await _userRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating User {userFromRepo.Id} to the database");
            }

            return NoContent();
        }

        // PUT: api/Users/{guid}
        /// <summary>
        /// Updates a single User. Only accessible by Administrators
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     PUT api/Users/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "firstname" :"Test-Title",
	    ///        "lastname": "Test Message",
	    ///        "reminder": "false",
	    ///        "email": "abc@gmail.com"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Id of the User to update</param>
        /// <param name="userToUpdate">User that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user updating User is not the owner of it</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminUser")]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UserUpdateDto userToUpdate)
        {
            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var userFromRepo = await _userRepository.GetUserAsync(User.GetUserId());

            if (userFromRepo == null)
            {
                return NotFound();
            }

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (userFromRepo?.Id != id ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot modify other users unless you are an administrator." });
            }

            _mapper.Map(userToUpdate, userFromRepo); //map fields from UserUpdateDto to User that was fetched from repository
            _userRepository.UpdateUser(userFromRepo); //Call to empty method. This is just for information and is not required

            if (!await _userRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating User {userFromRepo.Id} to the database");
            }

            return NoContent();
        }

        //PATCH: api/Users
        /// <summary>
        /// Updates the currently logged in User with a JSON patch document
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     PATCH api/Users/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "op": "replace",
		///        "path": "/firstname",
		///        "value": "Game of thrones User"
        ///     }
        ///
        /// </remarks>
        /// <param name="patchDoc">User as a patch document that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user deleting User is not the owner of it</response>
        [HttpPatch]
        public async Task<IActionResult> PartialUpdateUser([FromBody] JsonPatchDocument<UserUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var userFromRepo = await _userRepository.GetUserAsync(User.GetUserId());

            if (userFromRepo == null)
            {
                return NotFound();
            }

            var userToPatch = _mapper.Map<UserUpdateDto>(userFromRepo);

            patchDoc.ApplyTo(userToPatch, ModelState); //apply patch to user for update

            TryValidateModel(userToPatch); //Need to call this as otherwise the patch document will
                                           //be the only thing that is validated for invalid model state
                                           //Need to validate the actual user model after applying
                                           //the patch document to it to verify for any issues

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState); //need to explicitly call this after TryValidateModel
            }

            _mapper.Map(userToPatch, userFromRepo); //map values from patched user to user from repository
            _userRepository.UpdateUser(userFromRepo); //not required to call. For informational purposes

            if (!await _userRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating User {userFromRepo.Id} to the database");
            }

            return NoContent();
        }

        // DELETE: api/Users/{guid}
        /// <summary>
        /// Deletes a single User. Only accessible by Administrators
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     DELETE api/Users/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the User to delete</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the User was not found</response>
        /// <response code="403">Returns 403 if user updating User is not the owner of it</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminUser")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var userFromRepo = await _userRepository.GetUserAsync(id);

            if (userFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != userFromRepo.Id ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot delete other users unless you are an administrator." });
            }

            _userRepository.DeleteUser(userFromRepo);
            if (!await _userRepository.SaveChangesAsync())
            {
                throw new Exception($"Error deleting User {userFromRepo.Id} from the database");
            }

            return NoContent();
        }

        //This action exists to block user creation by posting with an Id. This is to adhere to
        //REST practices
        /// <summary>
        /// This action exists to block User creation by posting with an Id. This is to adhere to REST practices
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/Users/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the User</param>
        /// <returns>No Content</returns>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="409">Returns 409 if User already exists</response>
        [HttpPost("{id}")]
        public async Task<IActionResult> BlockUserCreation(Guid id)
        {
            if (await _userRepository.UserExistsAsync(id))
            {
                return StatusCode(409, new
                {
                    Error = "An user with this Id already exists. " +
                     "You cannot create resources by posting to this endpoint"
                });
            }

            return NotFound();
        }
    }
}