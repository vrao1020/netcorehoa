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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BoardMeetingsController : ControllerBase
    {
        private IBoardMeetingRepository _boardMeetingRepository;
        private IMapper _mapper;
        private IUserRepository _userRepository;
        private ISortFilterService<BoardMeeting> _sortFilterService;

        public BoardMeetingsController(IBoardMeetingRepository boardMeetingRepository, IMapper mapper,
            IUserRepository userRepository, ISortFilterService<BoardMeeting> sortFilterService)
        {
            _boardMeetingRepository = boardMeetingRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _sortFilterService = sortFilterService;
        }

        // GET: api/BoardMeetings
        /// <summary>
        /// Returns a list of Board Meetings. Returned items are paginated with a default page size
        /// of 5 items upto a max of 10 items.
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/BoardMeetings
        ///     ?sorts=         Title,Message         // sort by title and then by message 
        ///     &#38;filters=   OwnerEmail@=abc,      // filter to boardMeetings that contains the phrase "abc"
        ///     &#38;page=      1                     // get the first page...
        ///     &#38;pageSize=  10                    // ...which contains 10 BoardMeetings
        ///
        /// </remarks>
        /// <param name="sieveModel.Filters">See sample request for example of the input parameter</param>
        /// <returns>A list of Board Meetings that are paginated</returns>
        /// <response code="200">Returns a list of BoardMeetings</response>
        [HttpGet(Name = "GetBoardMeetings")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<IEnumerable<BoardMeetingViewDto>>> GetBoardMeetings([FromQuery] SieveModel sieveModel)
        {
            //throw new Exception($"Error saving BoardMeeting to the database");
            //var boardMeetings = await _boardMeetingRepository.GetBoardMeetingsAsync(sieveModel);
            //var boardMeetingsToReturn = _mapper.Map<IEnumerable<BoardMeetingViewDto>>(boardMeetings);

            var boardMeetings = _boardMeetingRepository.GetBoardMeetings();
            var boardMeetingsToReturn = _mapper.Map<IEnumerable<BoardMeetingViewDto>>(await _sortFilterService.ApplySortsFilters(boardMeetings, sieveModel));

            return Ok(boardMeetingsToReturn);
        }

        //GET: api/BoardMeetings/{guid}
        /// <summary>
        /// Returns a single Board Meeting if it exists or a 404 if it does not
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/BoardMeetings/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the BoardMeeting to return</param>
        /// <returns>A single Board Meeting</returns>
        /// <response code="200">Returns the BoardMeeting requested</response>
        /// <response code="404">Returns a 404 if requested BoardMeeting does not exist</response>
        [HttpGet("{id}", Name = "GetBoardMeeting")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<BoardMeetingViewDto>> GetBoardMeeting([FromRoute] Guid id)
        {
            var boardMeetingFromRepo = await _boardMeetingRepository.GetBoardMeetingAsync(id);

            if (boardMeetingFromRepo == null)
            {
                return NotFound();
            }

            var boardMeetingToReturn = _mapper.Map<BoardMeetingViewDto>(boardMeetingFromRepo);

            return Ok(boardMeetingToReturn);
        }

        //GET: api/BoardMeetings/GetCurrentBoardMeetingDue
        /// <summary>
        /// Returns a single Board Meeting that is is scheduled next (scheduled time is future dated and closest to current date)
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/BoardMeetings/GetCurrentBoardMeetingDue
        ///
        /// </remarks>
        /// <returns>A single Board Meeting that is scheduled next</returns>
        /// <response code="200">Returns the Board Meeting requested</response>
        [HttpGet("GetCurrentBoardMeetingDue")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<BoardMeetingViewDto>> GetCurrentBoardMeetingDue()
        {
            var boardMeetingFromRepo = await _boardMeetingRepository.GetCurrentDueBoardMeetingAsync();
            var boardMeetingToReturn = _mapper.Map<BoardMeetingViewDto>(boardMeetingFromRepo);

            return Ok(boardMeetingToReturn);
        }

        // POST: api/BoardMeetings
        /// <summary>
        /// Creates a single boardMeeting 
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/BoardMeetings
        ///     {
        ///        "title" :"Test-Title",
	    ///        "description": "Test Message",
	    ///        "scheduledTime": "2018-11-02T09:09:34.7209282-07:00"
        ///     }
        ///
        /// </remarks>
        /// <param name="boardMeetingInputDto">BoardMeeting to create</param>
        /// <returns>A newly created BoardMeeting</returns>
        /// <response code="201">Returns the newly created BoardMeeting</response>
        /// <response code="400">Returns 400 if the input is incorrect</response>
        [HttpPost]
        [Produces("application/json", "application/xml")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<ActionResult<BoardMeetingViewDto>> CreateBoardMeeting([FromBody] BoardMeetingInputDto boardMeetingInputDto)
        {
            if (boardMeetingInputDto == null)
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

            var boardMeetingToAdd = _mapper.Map<BoardMeeting>(boardMeetingInputDto); //map BoardMeetingInputDto to BoardMeeting
            boardMeetingToAdd.UserId = user.Id; //set the user id as otherwise navigation property will be null 

            if (boardMeetingInputDto.MeetingNotes != null)
            {
                var meetingMinuteToAdd = _mapper.Map<MeetingMinute>(boardMeetingInputDto.MeetingNotes); //map MeetingMinuteInputDto to MeetingMinute
                boardMeetingToAdd.MeetingNotes = meetingMinuteToAdd; //set the user id as otherwise navigation property will be null
                meetingMinuteToAdd.UserId = user.Id; //add the MeetingMinute to the BoardMeeting 
            }

            _boardMeetingRepository.AddBoardMeeting(boardMeetingToAdd);

            if (!await _boardMeetingRepository.SaveChangesAsync())
            {
                throw new Exception($"Error saving BoardMeeting {boardMeetingToAdd.Id} to the database");
            }

            var boardMeetingToReturn = _mapper.Map<BoardMeetingViewDto>(boardMeetingToAdd);

            return CreatedAtRoute("GetBoardMeeting", new { id = boardMeetingToAdd.Id }, boardMeetingToReturn);
        }

        // PUT: api/BoardMeetings/{guid}
        /// <summary>
        /// Updates a single boardMeeting 
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     PUT api/BoardMeetings/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "title" :"Test-Title",
	    ///        "message": "Test Message",
	    ///        "url": "https://abc.windows.net@xyz.com",
	    ///        "scheduledTime": "2018-11-02T09:09:34.7209282-07:00"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Id of the BoardMeeting to update</param>
        /// <param name="boardMeetingToUpdate">BoardMeeting that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user updating BoardMeeting is not the owner of it</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> UpdateBoardMeeting([FromRoute] Guid id,
            [FromBody] BoardMeetingUpdateDto boardMeetingToUpdate)
        {
            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            var boardMeetingFromRepo = await _boardMeetingRepository.GetBoardMeetingAsync(id);

            if (boardMeetingFromRepo == null)
            {
                return NotFound();
            }

            var meetingMinutes = boardMeetingFromRepo.MeetingNotes;

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != boardMeetingFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot modify other users submissions unless you are an administrator." });
            }

            _mapper.Map(boardMeetingToUpdate, boardMeetingFromRepo); //map fields from BoardMeetingUpdateDto to BoardMeeting that was fetched from repository
            _boardMeetingRepository.UpdateBoardMeeting(boardMeetingFromRepo); //Call to empty method. This is just for information and is not required

            if (boardMeetingToUpdate.MeetingNotes == null)
            {
                boardMeetingFromRepo.MeetingNotes = meetingMinutes;
            }

            if (!await _boardMeetingRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating BoardMeeting {boardMeetingFromRepo.Id} to the database");
            }

            return NoContent();
        }

        //PATCH: api/BoardMeetings/{guid}
        /// <summary>
        /// Updates a single boardMeeting with a JSON patch document
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     PATCH api/BoardMeetings/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "op": "replace",
		///        "path": "/title",
		///        "value": "Game of thrones boardMeeting"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Id of the BoardMeeting to update</param>
        /// <param name="patchDoc">BoardMeeting as a patch document that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user deleting BoardMeeting is not the owner of it</response>
        [HttpPatch("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> PartialUpdateBoardMeeting([FromRoute] Guid id,
            [FromBody] JsonPatchDocument<BoardMeetingUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            var boardMeetingFromRepo = await _boardMeetingRepository.GetBoardMeetingAsync(id);

            if (boardMeetingFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != boardMeetingFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot modify other users submissions unless you are an administrator." });
            }

            var boardMeetingToPatch = _mapper.Map<BoardMeetingUpdateDto>(boardMeetingFromRepo);

            patchDoc.ApplyTo(boardMeetingToPatch, ModelState); //apply patch to boardMeeting for update

            TryValidateModel(boardMeetingToPatch); //Need to call this as otherwise the patch document will
                                                   //be the only thing that is validated for invalid model state
                                                   //Need to validate the actual boardMeeting model after applying
                                                   //the patch document to it to verify for any issues

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState); //need to explicitly call this after TryValidateModel
            }

            _mapper.Map(boardMeetingToPatch, boardMeetingFromRepo); //map values from patched boardMeeting to boardMeeting from repository
            _boardMeetingRepository.UpdateBoardMeeting(boardMeetingFromRepo); //not required to call. For informational purposes

            if (!await _boardMeetingRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating BoardMeeting {boardMeetingFromRepo.Id} to the database");
            }

            return NoContent();

        }

        // DELETE: api/BoardMeetings/{guid}
        /// <summary>
        /// Deletes a single boardMeeting
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     DELETE api/BoardMeetings/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the BoardMeeting to delete</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the BoardMeeting was not found</response>
        /// <response code="403">Returns 403 if user updating BoardMeeting is not the owner of it</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> DeleteBoardMeeting(Guid id)
        {
            var boardMeetingFromRepo = await _boardMeetingRepository.GetBoardMeetingAsync(id);

            if (boardMeetingFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != boardMeetingFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot delete other users submissions unless you are an administrator." });
            }

            _boardMeetingRepository.DeleteBoardMeeting(boardMeetingFromRepo);

            if (!await _boardMeetingRepository.SaveChangesAsync())
            {
                throw new Exception($"Error deleting BoardMeeting {boardMeetingFromRepo.Id} from the database");
            }

            return NoContent();
        }

        //This action exists to block boardMeeting creation by posting with an Id. This is to adhere to
        //REST practices
        /// <summary>
        /// This action exists to block boardMeeting creation by posting with an Id. This is to adhere to REST practices
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/BoardMeetings/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the BoardMeeting</param>
        /// <returns>No Content</returns>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="409">Returns 409 if BoardMeeting already exists</response>
        [HttpPost("{id}")]
        public async Task<IActionResult> BlockBoardMeetingCreation(Guid id)
        {
            if (await _boardMeetingRepository.BoardMeetingExistsAsync(id))
            {
                return StatusCode(409, new
                {
                    Error = "An boardMeeting with this Id already exists. " +
                     "You cannot create resources by posting to this endpoint"
                });
            }

            return NotFound();
        }
    }
}