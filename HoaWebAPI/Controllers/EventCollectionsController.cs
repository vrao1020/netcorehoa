using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HoaEntities.Entities;
using HoaWebAPI.Extensions.ModelBinders;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.OutputModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HoaCommon.Extensions.UserClaims;
using HoaInfrastructure.Repositories;

namespace HoaWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventCollectionsController : ControllerBase
    {
        private IEventRepository _eventRepository;
        private IMapper _mapper;
        private IUserRepository _userRepository;

        public EventCollectionsController(IEventRepository eventRepository, IMapper mapper,
            IUserRepository userRepository)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        // GET: api/EventCollections
        /// <summary>
        /// Returns a list of Events
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/EventCollections/e77551ba-78e2-4a36-8754-3ea5f12e1619,e77551ba-78e2-4a36-8754-3ea5f12e1618
        ///
        /// </remarks>
        /// <param name="ids">A list of comma separated GUID</param>
        /// <returns>A list of requested Events</returns>
        /// <response code="200">Returns a list of Events</response>
        /// <response code="404">Returns 404 if requested Events don't exist</response>
        [HttpGet("{ids}", Name = "GetEventCollection")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<IEnumerable<EventViewDto>>> GetEventCollection([ModelBinder(BinderType = typeof(CSVModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var eventsFromRepo = await _eventRepository.GetEventsAsync(ids);

            if (eventsFromRepo?.Count() != ids.Count())
            {
                return NotFound(new { Error = "The repository does not contain the list of events requested" });
            }

            var eventsToReturn = _mapper.Map<IEnumerable<EventViewDto>>(eventsFromRepo);

            return Ok(eventsToReturn);
        }

        // POST: api/EventCollections
        /// <summary>
        /// Creates a collections of Events
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     POST api/EventCollections
        ///     [{
        ///         "title": "Test1",
        ///         "message": "Test Message",
        ///         "url": "https://abc.windows.net@xyz.com",
        ///         "scheduledTime": "2018-10-31T16:22:15.9339311-07:00",
        ///         "userId" : "5f76bd52-b065-487a-89ca-c9ec6a9b60c9"
        ///     },
        ///     {
        ///         "title": "TEST2",
        ///         "message": "TEST",
        ///         "url": "a",
        ///         "scheduledTime": "2018-10-31T16:22:15.9339311-07:00",
        ///         "userId" : "5f76bd52-b065-487a-89ca-c9ec6a9b60c9"
        ///     }]
        ///
        /// </remarks>
        /// <param name="events">Events to create</param>
        /// <returns>A newly created Event collection</returns>
        /// <response code="201">Returns the newly created Events collection</response>
        /// <response code="400">Returns 400 if the input is incorrect</response>
        [HttpPost]
        [Authorize(Policy = "CRUDAccess")]
        public async Task<ActionResult<IEnumerable<EventViewDto>>> CreateEventCollection([FromBody] IEnumerable<EventInputDto> events)
        {
            if (events == null)
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

            var eventsToAdd = _mapper.Map<IEnumerable<Event>>(events);

            foreach (var eventToAdd in eventsToAdd)
            {
                eventToAdd.UserId = user.Id;
                _eventRepository.AddEvent(eventToAdd);
            }

            if (!await _eventRepository.SaveChangesAsync())
            {
                throw new Exception($"Error adding Events to the database");
            }

            var eventsToReturn = _mapper.Map<IEnumerable<EventViewDto>>(eventsToAdd);
            var userIds = String.Join(",", eventsToReturn.Select(x => x.Id));

            return CreatedAtRoute("GetEventCollection", new { ids = userIds }, eventsToReturn);
        }
    }
}
