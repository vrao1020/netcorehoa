using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using HoaEntities.Entities;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.UpdateModels;
using HoaInfrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using HoaCommon.Extensions.UserClaims;
using HoaWebAPI.Services;

namespace HoaWebAPI.Controllers
{
    [Route("api/boardmeetings/{boardMeetingId}/[controller]")]
    [ApiController]
    [Authorize]
    public class MeetingMinutesController : ControllerBase
    {
        private IMeetingMinuteRepository _meetingMinuteRepository;
        private IMapper _mapper;
        private IUserRepository _userRepository;
        private IBoardMeetingRepository _boardMeetingRepository;
        private ISortFilterService<MeetingMinute> _sortFilterService;

        public MeetingMinutesController(IMeetingMinuteRepository meetingMinuteRepository, IMapper mapper,
            IUserRepository userRepository, IBoardMeetingRepository boardMeetingRepository, ISortFilterService<MeetingMinute> sortFilterService)
        {
            _meetingMinuteRepository = meetingMinuteRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _boardMeetingRepository = boardMeetingRepository;
            _sortFilterService = sortFilterService;
        }

        // GET: api/MeetingMinutes
        /// <summary>
        /// Returns a list of MeetingMinutes. Returned items are paginated with a default page size
        /// of 5 items upto a max of 10 items.
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/MeetingMinutes
        ///     ?sorts=         FileName              // sort by name and then by url 
        ///     &#38;filters=   OwnerEmail@=abc,      // filter to events that contains the phrase "abc"
        ///     &#38;page=      1                     // get the first page...
        ///     &#38;pageSize=  10                    // ...which contains 10 Events
        ///
        /// </remarks>
        /// <param name="sieveModel.Filters">See sample request for example of the input parameter</param>
        /// <returns>A list of MeetingMinutes that are paginated</returns>
        /// <response code="200">Returns a list of MeetingMinutes</response>
        [Route("~/api/[controller]")] //override the route set at the controller by using a ~ in front
        [HttpGet(Name = "GetAllMeetingMinutes")]
        [Produces("application/json", "application/xml")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<IEnumerable<MeetingMinuteViewDto>>> GetAllMeetingMinutes([FromQuery] SieveModel sieveModel)
        {
            var meetingMinutes = _meetingMinuteRepository.GetMeetingMinutes();
            var meetingMinutesToReturn = _mapper.Map<IEnumerable<MeetingMinuteViewDto>>(await _sortFilterService.ApplySortsFilters(meetingMinutes, sieveModel));

            return Ok(meetingMinutesToReturn);
        }

        //GET: api/1/MeetingMinutes/{guid}
        /// <summary>
        /// Returns a single MeetingMinute if it exists or a 404 if it does not
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/BoardMeetings/e77551ba-78e3-4a36-8754-3ea5f12e1619/MeetingMinutes/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="boardMeetingId">Id of the board meeting</param>
        /// <param name="id">Id of the MeetingMinute to return</param>
        /// <returns>A single MeetingMinute</returns>
        /// <response code="200">Returns the MeetingMinute requested</response>
        /// <response code="404">Returns a 404 if requested MeetingMinute does not exist</response>
        [HttpGet("{id}", Name = "GetMeetingMinute")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<MeetingMinuteViewDto>> GetMeetingMinute([FromRoute] Guid boardMeetingId, [FromRoute] Guid id)
        {
            if (!await _boardMeetingRepository.BoardMeetingExistsAsync(boardMeetingId))
            {
                return NotFound();
            }

            var meetingMinuteFromRepo = await _meetingMinuteRepository.GetMeetingMinuteAsync(boardMeetingId, id);

            if (meetingMinuteFromRepo == null)
            {
                return NotFound();
            }

            var meetingMinuteToReturn = _mapper.Map<MeetingMinuteViewDto>(meetingMinuteFromRepo);

            return Ok(meetingMinuteToReturn);
        }

        // POST: api/1/MeetingMinutes
        /// <summary>
        /// Creates a single MeetingMinutes 
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/BoardMeetings/e77551ba-78e3-4a36-8754-3ea5f12e1619/MeetingMinutes
        ///     {
        ///        "FileName" :"Test-Title.txt"
        ///     }
        ///
        /// </remarks>
        /// <param name="boardMeetingId">Id of the board meeting</param>
        /// <param name="meetingMinuteInputDto">MeetingMinute to create</param>
        /// <returns>A newly created MeetingMinute</returns>
        /// <response code="201">Returns the newly created MeetingMinute</response>
        /// <response code="400">Returns 400 if the input is incorrect</response>
        /// <response code="409">Returns 409 if the board meeting already contains a meeting minute</response>
        [HttpPost]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<ActionResult<MeetingMinuteViewDto>> CreateMeetingMinute([FromRoute] Guid boardMeetingId, [FromBody] MeetingMinuteInputDto meetingMinuteInputDto)
        {
            if (meetingMinuteInputDto == null)
            {
                return BadRequest();
            }

            var boardMeeting = await _boardMeetingRepository.GetBoardMeetingAsync(boardMeetingId);

            if (boardMeeting == null)
            {
                return NotFound();
            }

            if (boardMeeting.MeetingNotes != null)
            {
                return StatusCode(409, new { Error = "A meeting minute already exists for this board meeting" });
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

            var meetingMinuteToAdd = _mapper.Map<MeetingMinute>(meetingMinuteInputDto); //map EventInputDto to Event
            meetingMinuteToAdd.UserId = user.Id; //set the user id as otherwise navigation property will be null 
            await _meetingMinuteRepository.AddMeetingMinute(boardMeetingId, meetingMinuteToAdd);

            if (!await _meetingMinuteRepository.SaveChangesAsync())
            {
                throw new Exception($"Error saving Event {meetingMinuteToAdd.Id} to the database");
            }

            var meetingMinuteToReturn = _mapper.Map<MeetingMinuteViewDto>(meetingMinuteToAdd);

            return CreatedAtRoute("GetMeetingMinute", new { boardMeetingId = boardMeetingId, id = meetingMinuteToAdd.Id }, meetingMinuteToReturn);
        }

        // PUT: api/1/MeetingMinutes/{guid}
        /// <summary>
        /// Updates a single MeetingMinute 
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     PUT api/BoardMeetings/e77551ba-78e3-4a36-8754-3ea5f12e1619/MeetingMinutes/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "FileName" :"Test-Title.txt"
        ///     }
        ///
        /// </remarks>
        /// <param name="boardMeetingId">Id of the board meeting</param>
        /// <param name="id">Id of the MeetingMinute to update</param>
        /// <param name="meetingMinuteToUpdate">MeetingMinute that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user updating MeetingMinute is not the owner of it</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> UpdateMeetingMinute([FromRoute] Guid boardMeetingId, [FromRoute] Guid id,
            [FromBody] MeetingMinuteUpdateDto meetingMinuteToUpdate)
        {
            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            if (!await _boardMeetingRepository.BoardMeetingExistsAsync(boardMeetingId))
            {
                return NotFound();
            }

            var meetingMinuteFromRepo = await _meetingMinuteRepository.GetMeetingMinuteAsync(boardMeetingId, id);

            if (meetingMinuteFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != meetingMinuteFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot modify other users submissions unless you are an administrator." });
            }

            _mapper.Map(meetingMinuteToUpdate, meetingMinuteFromRepo); //map fields from MeetingMinuteUpdateDto to MeetingMinute that was fetched from repository
            _meetingMinuteRepository.UpdateMeetingMinute(boardMeetingId, meetingMinuteFromRepo); //Call to empty method. This is just for information and is not required

            if (!await _meetingMinuteRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating Event {meetingMinuteFromRepo.Id} to the database");
            }

            return NoContent();
        }

        //PATCH: api/1/MeetingMinutes/{guid}
        /// <summary>
        /// Updates a single MeetingMinute with a JSON patch document
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     PATCH api/BoardMeetings/e77551ba-78e3-4a36-8754-3ea5f12e1619/MeetingMinutes/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "op": "replace",
        ///        "path": "/filename",
        ///        "value": "Game of thrones event.txt"
        ///     }
        ///
        /// </remarks>
        /// <param name="boardMeetingId">Id of the board meeting</param>
        /// <param name="id">Id of the MeetingMinute to update</param>
        /// <param name="patchDoc">Event as a patch document that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user deleting MeetingMinute is not the owner of it</response>
        [HttpPatch("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> PartialUpdateMeetingMinute([FromRoute] Guid boardMeetingId, [FromRoute] Guid id,
            [FromBody] JsonPatchDocument<MeetingMinuteUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!await _boardMeetingRepository.BoardMeetingExistsAsync(boardMeetingId))
            {
                return NotFound();
            }

            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            var meetingMinuteFromRepo = await _meetingMinuteRepository.GetMeetingMinuteAsync(boardMeetingId, id);

            if (meetingMinuteFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != meetingMinuteFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot modify other users submissions unless you are an administrator." });
            }

            var meetingMinuteToPatch = _mapper.Map<MeetingMinuteUpdateDto>(meetingMinuteFromRepo);

            patchDoc.ApplyTo(meetingMinuteToPatch, ModelState); //apply patch to event for update

            TryValidateModel(meetingMinuteToPatch); //Need to call this as otherwise the patch document will
                                                    //be the only thing that is validated for invalid model state
                                                    //Need to validate the actual event model after applying
                                                    //the patch document to it to verify for any issues

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState); //need to explicitly call this after TryValidateModel
            }

            _mapper.Map(meetingMinuteToPatch, meetingMinuteFromRepo); //map values from patched event to event from repository
            _meetingMinuteRepository.UpdateMeetingMinute(boardMeetingId, meetingMinuteFromRepo); //not required to call. For informational purposes

            if (!await _meetingMinuteRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating Event {meetingMinuteFromRepo.Id} to the database");
            }

            return NoContent();

        }

        // DELETE: api/1/MeetingMinutes/{guid}
        /// <summary>
        /// Deletes a single MeetingMinute
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     DELETE api/BoardMeetings/e77551ba-78e3-4a36-8754-3ea5f12e1619/MeetingMinutes/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="boardMeetingId">Id of the board meeting</param>
        /// <param name="id">Id of the MeetingMinute to delete</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the Event was not found</response>
        /// <response code="403">Returns 403 if user deleting the MeetingMinute is not the owner of it</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> DeleteMeetingMinute([FromRoute] Guid boardMeetingId, Guid id)
        {
            if (!await _boardMeetingRepository.BoardMeetingExistsAsync(boardMeetingId))
            {
                return NotFound();
            }

            var meetingMinuteFromRepo = await _meetingMinuteRepository.GetMeetingMinuteAsync(boardMeetingId, id);

            if (meetingMinuteFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != meetingMinuteFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot delete other users submissions unless you are an administrator." });
            }

            _meetingMinuteRepository.DeleteMeetingMinute(meetingMinuteFromRepo);
            if (!await _meetingMinuteRepository.SaveChangesAsync())
            {
                throw new Exception($"Error deleting Event {meetingMinuteFromRepo.Id} from the database");
            }

            return NoContent();
        }

        //This action exists to block MeetingMinutes creation by posting with an Id. This is to adhere to
        //REST practices
        /// <summary>
        /// This action exists to block MeetingMinute creation by posting with an Id. This is to adhere to REST practices
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/BoardMeetings/e77551ba-78e3-4a36-8754-3ea5f12e1619/MeetingMinutes/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="boardMeetingId">Id of the board meeting</param>
        /// <param name="id">Id of the MeetingMinute</param>
        /// <returns>No Content</returns>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="409">Returns 409 if MeetingMinute already exists</response>
        [HttpPost("{id}")]
        public async Task<IActionResult> BlockMeetingMinuteCreation([FromRoute] Guid boardMeetingId, Guid id)
        {
            if (!await _boardMeetingRepository.BoardMeetingExistsAsync(boardMeetingId))
            {
                return NotFound();
            }

            if (await _meetingMinuteRepository.MeetingMinuteExistsAsync(boardMeetingId, id))
            {
                return StatusCode(409, new
                {
                    Error = "An event with this Id already exists. " +
                     "You cannot create resources by posting to this endpoint"
                });
            }

            return NotFound();
        }
    }
}