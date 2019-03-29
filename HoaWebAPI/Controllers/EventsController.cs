using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HoaInfrastructure.Repositories;
using AutoMapper;
using HoaEntities.Models.OutputModels;
using System.Collections.Generic;
using System;
using HoaEntities.Models.InputModels;
using HoaEntities.Entities;
using HoaEntities.Models.UpdateModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using Sieve.Models;
using HoaCommon.Extensions.UserClaims;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using HoaWebAPI.Services;

namespace HoaWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private IEventRepository _eventRepository;
        private IMapper _mapper;
        private IUserRepository _userRepository;
        private IDistributedCache _cache;
        private ISortFilterService<Event> _sortFilterService;

        public EventsController(IEventRepository eventRepository, IMapper mapper,
            IUserRepository userRepository, IDistributedCache cache, ISortFilterService<Event> sortFilterService)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _cache = cache;
            _sortFilterService = sortFilterService;
        }

        // GET: api/Events
        /// <summary>
        /// Returns a list of events. Returned items are paginated with a default page size
        /// of 5 items upto a max of 10 items.
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/Events
        ///     ?sorts=         Title,Message         // sort by title and then by message 
        ///     &#38;filters=   OwnerEmail@=abc,      // filter to events that contains the phrase "abc"
        ///     &#38;page=      1                     // get the first page...
        ///     &#38;pageSize=  10                    // ...which contains 10 Events
        ///
        /// </remarks>
        /// <param name="sieveModel.Filters">See sample request for example of the input parameter</param>
        /// <returns>A list of events that are paginated</returns>
        /// <response code="200">Returns a list of Events</response>
        [HttpGet(Name = "GetEvents")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<IEnumerable<EventViewDto>>> GetEvents([FromQuery] SieveModel sieveModel)
        {
            var events = _eventRepository.GetEvents();

            //to be determined if below is needed
            ////loop through each event returned and check the message length        
            //foreach (var item in events)
            //{
            //    //if greater than 100, cut it down to 100 characters and append ... at the end
            //    //this is because on the front end, user can then click into the detail if they want
            //    //to read more about the event
            //    if (item.Message.Length > 100)
            //    {
            //        item.Message = item.Message.Substring(0, 100) + "...";
            //    }
            //}

            var eventsToReturn = _mapper.Map<IEnumerable<EventViewDto>>(await _sortFilterService.ApplySortsFilters(events, sieveModel));

            return Ok(eventsToReturn);
        }

        //GET: api/Events/{guid}
        /// <summary>
        /// Returns a single Event if it exists or a 404 if it does not
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/Events/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the Event to return</param>
        /// <returns>A single Event</returns>
        /// <response code="200">Returns the Event requested</response>
        /// <response code="404">Returns a 404 if requested Event does not exist</response>
        [HttpGet("{id}", Name = "GetEvent")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<EventViewDto>> GetEvent([FromRoute] Guid id)
        {
            var eventFromRepo = await _eventRepository.GetEventAsync(id);

            if (eventFromRepo == null)
            {
                return NotFound();
            }

            var eventToReturn = _mapper.Map<EventViewDto>(eventFromRepo);

            return Ok(eventToReturn);
        }

        //GET: api/Events/GetCurrentEventDue
        /// <summary>
        /// Returns a single Event that is is scheduled next (scheduled time is future dated and closest to current date)
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/Events/GetCurrentEventDue
        ///
        /// </remarks>
        /// <returns>A single Event that is scheduled next</returns>
        /// <response code="200">Returns the Event requested</response>
        [HttpGet("GetCurrentEventDue")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<EventViewDto>> GetCurrentEventDue()
        {
            var eventFromRepo = await _eventRepository.GetCurrentDueEventAsync();
            var eventToReturn = _mapper.Map<EventViewDto>(eventFromRepo);

            return Ok(eventToReturn);
        }

        // POST: api/Events
        /// <summary>
        /// Creates a single event 
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/Events
        ///     {
        ///        "title" :"Test-Title",
	    ///        "message": "Test Message",
	    ///        "url": "https://abc.windows.net@xyz.com",
	    ///        "scheduledTime": "2018-11-02T09:09:34.7209282-07:00"
        ///     }
        ///
        /// </remarks>
        /// <param name="eventInputDto">Event to create</param>
        /// <returns>A newly created Event</returns>
        /// <response code="201">Returns the newly created Event</response>
        /// <response code="400">Returns 400 if the input is incorrect</response>
        [HttpPost]
        [Produces("application/json", "application/xml")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<ActionResult<EventViewDto>> CreateEvent([FromBody] EventInputDto eventInputDto)
        {
            if (eventInputDto == null)
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

            var eventToAdd = _mapper.Map<Event>(eventInputDto); //map EventInputDto to Event
            eventToAdd.UserId = user.Id; //set the user id as otherwise navigation property will be null 
            _eventRepository.AddEvent(eventToAdd);

            if (!await _eventRepository.SaveChangesAsync())
            {
                throw new Exception($"Error saving Event {eventToAdd.Id} to the database");
            }

            var eventToReturn = _mapper.Map<EventViewDto>(eventToAdd);

            return CreatedAtRoute("GetEvent", new { id = eventToAdd.Id }, eventToReturn);
        }

        // PUT: api/Events/{guid}
        /// <summary>
        /// Updates a single event 
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     PUT api/Events/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "title" :"Test-Title",
	    ///        "message": "Test Message",
	    ///        "url": "https://abc.windows.net@xyz.com",
	    ///        "scheduledTime": "2018-11-02T09:09:34.7209282-07:00"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Id of the Event to update</param>
        /// <param name="eventToUpdate">Event that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user updating Event is not the owner of it</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> UpdateEvent([FromRoute] Guid id,
            [FromBody] EventUpdateDto eventToUpdate)
        {
            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            var eventFromRepo = await _eventRepository.GetEventAsync(id);

            if (eventFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != eventFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot modify other users submissions unless you are an administrator." });
            }

            _mapper.Map(eventToUpdate, eventFromRepo); //map fields from EventUpdateDto to Event that was fetched from repository
            _eventRepository.UpdateEvent(eventFromRepo); //Call to empty method. This is just for information and is not required

            if (!await _eventRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating Event {eventFromRepo.Id} to the database");
            }

            return NoContent();
        }

        //PATCH: api/Events/{guid}
        /// <summary>
        /// Updates a single event with a JSON patch document
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     PATCH api/Events/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///     {
        ///        "op": "replace",
		///        "path": "/title",
		///        "value": "Game of thrones event"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">Id of the Event to update</param>
        /// <param name="patchDoc">Event as a patch document that needs to be updated</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="403">Returns 403 if user deleting Event is not the owner of it</response>
        [HttpPatch("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> PartialUpdateEvent([FromRoute] Guid id,
            [FromBody] JsonPatchDocument<EventUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            //model state validation is not required due to the [ApiController] attribute automatically returning UnprocessableEntity (see startup.cs)
            //when model binding fails

            var eventFromRepo = await _eventRepository.GetEventAsync(id);

            if (eventFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != eventFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot modify other users submissions unless you are an administrator." });
            }

            var eventToPatch = _mapper.Map<EventUpdateDto>(eventFromRepo);

            patchDoc.ApplyTo(eventToPatch, ModelState); //apply patch to event for update

            TryValidateModel(eventToPatch); //Need to call this as otherwise the patch document will
                                            //be the only thing that is validated for invalid model state
                                            //Need to validate the actual event model after applying
                                            //the patch document to it to verify for any issues

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState); //need to explicitly call this after TryValidateModel
            }

            _mapper.Map(eventToPatch, eventFromRepo); //map values from patched event to event from repository
            _eventRepository.UpdateEvent(eventFromRepo); //not required to call. For informational purposes

            if (!await _eventRepository.SaveChangesAsync())
            {
                throw new Exception($"Error updating Event {eventFromRepo.Id} to the database");
            }

            return NoContent();

        }

        // DELETE: api/Events/{guid}
        /// <summary>
        /// Deletes a single event
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     DELETE api/Events/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the Event to delete</param>
        /// <returns>No Content</returns>
        /// <response code="204">Returns 204 if successful</response>
        /// <response code="400">Returns 404 if the Event was not found</response>
        /// <response code="403">Returns 403 if user updating Event is not the owner of it</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var eventFromRepo = await _eventRepository.GetEventAsync(id);

            if (eventFromRepo == null)
            {
                return NotFound();
            }

            //fetch the user id from the JWT via HttpContext. Then get the user from the repository. This is to ensure that an authorized user
            //is calling the API with a valid user id
            var user = await _userRepository.GetUserAsync(User.GetUserId());

            //check if the user is an administrator or owner of the object
            var isAdmin = User?.IsAdministrator() ?? false;
            var userIsAdminOrOwner = isAdmin ? isAdmin : (user?.Id != eventFromRepo.UserId ? false : true);

            if (!userIsAdminOrOwner)
            {
                //returning status code instead of Forbid() as forbid only works with authentication handlers
                return StatusCode(403, new { Error = "You cannot delete other users submissions unless you are an administrator." });
            }

            _eventRepository.DeleteEvent(eventFromRepo);
            if (!await _eventRepository.SaveChangesAsync())
            {
                throw new Exception($"Error deleting Event {eventFromRepo.Id} from the database");
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
        ///     POST api/Events/e77551ba-78e2-4a36-8754-3ea5f12e1619
        ///
        /// </remarks>
        /// <param name="id">Id of the Event</param>
        /// <returns>No Content</returns>
        /// <response code="400">Returns 404 if the input is incorrect</response>
        /// <response code="409">Returns 409 if Event already exists</response>
        [HttpPost("{id}")]
        public async Task<IActionResult> BlockEventCreation(Guid id)
        {
            if (await _eventRepository.EventExistsAsync(id))
            {
                return StatusCode(409, new
                {
                    Error = "An event with this Id already exists. " +
                     "You cannot create resources by posting to this endpoint"
                });
            }

            return NotFound();
        }

        //[HttpGet("claims/test")]
        //[Authorize(Policy = "AdminUser")]
        //public IActionResult GetClaims()
        //{
        //    return Ok(User.Claims.Select( x => 
        //            new {
        //                x.Type,
        //                x.Value
        //            }
        //        ));
        //}

        [HttpGet("eventsredis")]
        [ProducesResponseType(200)]
        [Produces("application/json", "application/xml")]
        public async Task<IActionResult> GetEventsOther([FromQuery] SieveModel sieveModel)
        {
            //_cache.Remove("events");
            var cachedEvents = await _cache.GetStringAsync("events");
            IEnumerable<Event> events = null;

            if (!string.IsNullOrEmpty(cachedEvents))
            {
                events = JsonConvert.DeserializeObject<IEnumerable<Event>>(cachedEvents);
            }
            else
            {
                var eventsFromRepo = _eventRepository.GetEvents();
                events = await _sortFilterService.ApplySortsFilters(eventsFromRepo, sieveModel);

                string item = JsonConvert.SerializeObject(events, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                await _cache.SetStringAsync("events", item);
            }

            var eventsToReturn = _mapper.Map<IEnumerable<EventViewDto>>(events);
            return Ok(eventsToReturn);
        }
    }
}