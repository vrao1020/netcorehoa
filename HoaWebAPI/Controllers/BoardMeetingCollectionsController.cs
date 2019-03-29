using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HoaWebAPI.Extensions.ModelBinders;
using HoaEntities.Models.OutputModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HoaInfrastructure.Repositories;

namespace HoaWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BoardMeetingCollectionsController : ControllerBase
    {
        private IBoardMeetingRepository _boardMeetingRepository;
        private IMapper _mapper;
        private IUserRepository _userRepository;

        public BoardMeetingCollectionsController(IBoardMeetingRepository boardMeetingRepository, IMapper mapper,
            IUserRepository userRepository)
        {
            _boardMeetingRepository = boardMeetingRepository;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        // GET: api/BoardMeetingCollections
        /// <summary>
        /// Returns a list of Board Meetings
        /// </summary>
        ///<remarks>
        /// Sample request:
        ///
        ///     GET api/BoardMeetingCollections/e77551ba-78e2-4a36-8754-3ea5f12e1619,e77551ba-78e2-4a36-8754-3ea5f12e1618
        ///
        /// </remarks>
        /// <param name="ids">A list of comma separated GUID</param>
        /// <returns>A list of requested BoardMeetings</returns>
        /// <response code="200">Returns a list of BoardMeetings</response>
        /// <response code="404">Returns 404 if requested BoardMeetings don't exist</response>
        [HttpGet("{ids}", Name = "GetBoardMeetingCollection")]
        [Produces("application/json", "application/xml")]
        public async Task<ActionResult<IEnumerable<BoardMeetingViewDto>>> GetBoardMeetingCollection([ModelBinder(BinderType = typeof(CSVModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var boardMeetingsFromRepo = await _boardMeetingRepository.GetBoardMeetingsAsync(ids);

            if (boardMeetingsFromRepo?.Count() != ids.Count())
            {
                return NotFound(new { Error = "The repository does not contain the list of board meetings requested" });
            }

            var boardMeetingsToReturn = _mapper.Map<IEnumerable<BoardMeetingViewDto>>(boardMeetingsFromRepo);

            return Ok(boardMeetingsToReturn);
        }
    }
}
