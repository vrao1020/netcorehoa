using AutoMapper;
using HoaWebAPI.Controllers;
using HoaEntities.Entities;
using HoaEntities.Models.InputModels;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.UpdateModels;
using HoaInfrastructure.Repositories;
using HoaWebAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HoaWebAPIUnitTests.ControllerTests
{
    public class EventControllerShould
    {

        private EventsController eventsController;
        private Mock<IEventRepository> eventRepository;
        private Mock<IUserRepository> userRepository;
        private Mock<IMapper> mapper;
        private Mock<SieveModel> sieveModel;
        private Mock<EventInputDto> eventInputDto;
        private Mock<EventUpdateDto> eventUpdateDto;
        private Mock<JsonPatchDocument<EventUpdateDto>> jsonPatchEventUpdateDto;
        private Mock<IDistributedCache> distributedCache;
        private Mock<ISortFilterService<Event>> sortFilterService;

        public EventControllerShould()
        {
            eventRepository = new Mock<IEventRepository>();
            userRepository = new Mock<IUserRepository>();
            mapper = new Mock<IMapper>();
            sieveModel = new Mock<SieveModel>();
            eventInputDto = new Mock<EventInputDto>();
            eventUpdateDto = new Mock<EventUpdateDto>();
            jsonPatchEventUpdateDto = new Mock<JsonPatchDocument<EventUpdateDto>>();
            distributedCache = new Mock<IDistributedCache>();
            sortFilterService = new Mock<ISortFilterService<Event>>();

            // Arrange
            eventRepository.Setup(repo => repo.GetEventAsync(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"))).ReturnsAsync(GetTestEvent());
            eventRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => true);
            eventRepository.Setup(repo => repo.EventExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            eventRepository.Setup(repo => repo.GetEvents()).Returns(() => null);

            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);

            mapper.Setup(map => map.Map<EventViewDto>(It.IsAny<Event>())).Returns(GetTestEventViewDto);
            mapper.Setup(map => map.Map<Event>(It.IsAny<EventInputDto>())).Returns(GetTestEvent);
            mapper.Setup(map => map.Map<IEnumerable<EventViewDto>>(It.IsAny<IEnumerable<Event>>())).Returns(GetTestEventsViewDto);
            mapper.Setup(map => map.Map<EventUpdateDto>(It.IsAny<Event>())).Returns(GetTestEventUpdateDto);

            eventsController = new EventsController(eventRepository.Object, mapper.Object, userRepository.Object, distributedCache.Object, sortFilterService.Object);

            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);
        }

        [Fact]
        public async Task GetSingleEvent_ReturnsNotFound_WhenInvalidGuidUsed()
        {
            // Arrange

            //Act
            var result = await eventsController.GetEvent(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1699"));

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task GetSingleEvent_ReturnsNotNull_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await eventsController.GetEvent(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSingleEvent_ReturnsOkResult_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await eventsController.GetEvent(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetSingleEvent_ReturnsEventViewDto_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await eventsController.GetEvent(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.IsAssignableFrom<EventViewDto>(okResult.Value);
        }


        [Fact]
        public async Task GetSingleEvent_ReturnsRightEvent_WhenValidGuidUsed()
        {
            // Arrange
            var guid = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619");

            //Act
            var controllerresult = await eventsController.GetEvent(guid);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var model = Assert.IsAssignableFrom<EventViewDto>(result.Value);
            Assert.Equal(guid, model.Id);
        }

        [Fact]
        public async Task GetEvents_ReturnsOkResult()
        {
            // Arrange

            //Act
            var result = await eventsController.GetEvents(sieveModel.Object);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetEvents_Returns_IEnumerableOfEventViewDto()
        {
            // Arrange

            //Act            
            var controllerresult = await eventsController.GetEvents(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            Assert.IsAssignableFrom<IEnumerable<EventViewDto>>(result.Value);
        }

        [Fact]
        public async Task GetEvents_Returns_ExactlyTwo_EventViewDtos()
        {
            // Arrange

            //Act
            var controllerresult = await eventsController.GetEvents(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var events = Assert.IsAssignableFrom<IEnumerable<EventViewDto>>(result.Value);

            Assert.Equal(2, events.Count());
        }

        [Fact]
        public async Task GetEvents_Returns_ExactlyOne_EventViewDto_WithTitleFilter()
        {
            // Arrange
            sieveModel.Object.Filters = "Title@=*1";
            mapper.Setup(map => map.Map<IEnumerable<EventViewDto>>(It.IsAny<IEnumerable<Event>>())).Returns(GetTestEventsViewDtoSingle);

            //Act
            var controllerresult = await eventsController.GetEvents(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var events = Assert.IsAssignableFrom<IEnumerable<EventViewDto>>(result.Value);

            Assert.Single(events);
        }

        [Fact]
        public async Task Add_InvalidEvent_Returns_BadRequest()
        {
            //Arrange
            EventInputDto x = null;

            //Act
            var result = await eventsController.CreateEvent(x);

            //Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task Add_InvalidEvent_WithoutValidUser_Returns_BadRequest()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var result = await eventsController.CreateEvent(eventInputDto.Object);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Add_InvalidEvent_WithModelErrors_Returns_UnprocessableEntityRequest()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            eventsController.ModelState.AddModelError("X", "Invalid model state");

            //Act
            var result = await eventsController.CreateEvent(eventInputDto.Object);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Add_ValidEvent__Returns_CreatedAtRouteResult()
        {
            //Arrange

            //Act
            var result = await eventsController.CreateEvent(eventInputDto.Object);

            //Assert
            Assert.IsType<CreatedAtRouteResult>(result.Result);
        }

        [Fact]
        public async Task Add_ValidEvent__Returns_EventViewDto()
        {
            //Arrange

            //Act
            var resultfromcontroller = await eventsController.CreateEvent(eventInputDto.Object);
            var result = resultfromcontroller.Result as CreatedAtRouteResult;

            //Assert
            Assert.IsType<EventViewDto>(result.Value);
        }

        [Fact]
        public async Task Add_ValidEvent__Returns_CreatedAtRouteResult_At_GetEvent_Action()
        {
            //Arrange

            //Act
            var resultfromcontroller = await eventsController.CreateEvent(eventInputDto.Object);
            var result = resultfromcontroller.Result as CreatedAtRouteResult;

            //Assert
            Assert.Equal("GetEvent", result.RouteName);
        }

        [Fact]
        public async Task Checking_Existing_EventId__Returns_ObjectResult()
        {
            //Arrange

            //Act
            var result = await eventsController.BlockEventCreation(Guid.NewGuid());

            //Assert
            Assert.IsType<ObjectResult>(result);
        }

        [Fact]
        public async Task Checking_Existing_EventId__Returns_409StatusCode()
        {
            //Arrange

            //Act
            var result = await eventsController.BlockEventCreation(Guid.NewGuid()) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        }

        [Fact]
        public async Task Delete_Event_ThatDoesNotExist_Returns_NotFound()
        {
            //Arrange
            eventRepository.Setup(repo => repo.GetEventAsync(It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var result = await eventsController.DeleteEvent(Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Event_OfDifferentUser_Returns_ForbiddenStatusCode()
        {
            //Arrange
            eventRepository.Setup(repo => repo.GetEventAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestEvent);
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var result = await eventsController.DeleteEvent(Guid.NewGuid()) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Delete_Event_Returns_NoContent()
        {
            //Arrange
            eventRepository.Setup(repo => repo.GetEventAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestEvent);

            //Act
            var result = await eventsController.DeleteEvent(Guid.NewGuid());

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_WithInValidEvent_Returns_UnProcessableEntityResult()
        {
            //Arrange

            //Act
            var result = await eventsController.UpdateEvent(Guid.NewGuid(), eventUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_Event_Returns_NoContent()
        {
            //Arrange
            eventRepository.Setup(repo => repo.GetEventAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestEvent);

            //Act
            var result = await eventsController.UpdateEvent(Guid.NewGuid(), eventUpdateDto.Object);

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_EventNotInDatabase_Returns_NotFoundResult()
        {
            //Arrange

            //Act
            var result = await eventsController.UpdateEvent(Guid.NewGuid(), eventUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_DatabaseSaveError_Throws_Exception()
        {
            //Arrange
            eventRepository.Setup(repo => repo.GetEventAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestEvent);
            eventRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => false);

            //Act

            //Assert
            await Assert.ThrowsAsync<Exception>(() => eventsController.UpdateEvent(Guid.NewGuid(), eventUpdateDto.Object));
        }

        [Fact]
        public async Task Update_OnPatch_With_DatabaseSaveError_Throws_Exception()
        {
            //Arrange
            eventRepository.Setup(repo => repo.GetEventAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestEvent);
            eventRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => false);
            jsonPatchEventUpdateDto.Object.Replace(x => x.Title, "ABC");
            JsonPatchDocument<EventUpdateDto> test = new JsonPatchDocument<EventUpdateDto>();
            test.Replace(x => x.Title, "ABC");

            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(),
                                              It.IsAny<ValidationStateDictionary>(),
                                              It.IsAny<string>(),
                                              It.IsAny<Object>()))
                                              .Throws(new Exception());
            eventsController.ObjectValidator = objectValidator.Object;

            //Act

            //Assert
            await Assert.ThrowsAsync<Exception>(() => eventsController.PartialUpdateEvent(Guid.NewGuid(), test));
        }

        private EventUpdateDto GetTestEventUpdateDto()
        {
            return new EventUpdateDto
            {
                Title = "test title",
                Message = "Test message",
                ImageName = "https://abc.windows.net@xyz.com",
                ThumbnailName = "abc",
                ScheduledTime = DateTime.Now
            };
        }

        private EventInputDto GetEventForInput()
        {
            return new EventInputDto
            {
                Title = "test title",
                Message = "Test message",
                ImageName = "https://abc.windows.net@xyz.com",
                ThumbnailName = "abc",
                ScheduledTime = DateTime.Now
            };
        }

        private EventViewDto GetTestEventViewDto()
        {
            var events = new EventViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Message = "Test1",
                ImageName = "https://abc.windows.net@xyz.com",
                ThumbnailName = "abc",
                DaysOld = 0,
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com"
            };

            return events;
        }

        private IEnumerable<EventViewDto> GetTestEventsViewDto()
        {
            var events = new List<EventViewDto>();
            events.Add(new EventViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Message = "Test1",
                ImageName = "https://abc.windows.net@xyz.com",
                ThumbnailName = "abc",
                DaysOld = 0,
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com"
            });
            events.Add(new EventViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                Title = "Test2",
                Message = "Test2",
                ImageName = "https://abc.windows.net@xyz.com",
                ThumbnailName = "abc",
                DaysOld = 0,
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                OwnerName = "XYZ",
                OwnerEmail = "Test@test.com"
            });

            return events;
        }

        private IEnumerable<EventViewDto> GetTestEventsViewDtoSingle()
        {
            var events = new List<EventViewDto>();
            events.Add(new EventViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Message = "Test1",
                ImageName = "https://abc.windows.net@xyz.com",
                ThumbnailName = "abc",
                DaysOld = 0,
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com"
            });
            return events;
        }

        private Event GetTestEvent()
        {
            var events = new Event()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Message = "Test1",
                ImageName = "https://abc.windows.net@xyz.com",
                ThumbnailName = "abc",
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            };

            return events;
        }

        private IEnumerable<Event> GetTestEvents()
        {
            var events = new List<Event>();
            events.Add(new Event()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Message = "Test1",
                ImageName = "https://abc.windows.net@xyz.com",
                ThumbnailName = "abc",
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            });
            events.Add(new Event()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                Title = "Test2",
                Message = "Test2",
                ImageName = "https://abc.windows.net@xyz.com",
                ThumbnailName = "abc",
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            });
            return events.AsEnumerable();
        }

        private IEnumerable<Event> GetTestEventsSingleEvent()
        {
            var events = new List<Event>();
            events.Add(new Event()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Message = "Test1",
                ImageName = "https://abc.windows.net@xyz.com",
                ThumbnailName = "abc",
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            });
            return events.AsEnumerable();
        }

        private User GetTestUser()
        {
            return new User
            {
                Id = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9"),
                SocialId = "zqSdVijp7rK9hsg5BhKNvfOzNFxSBRcl@clients",
                FirstName = "A",
                LastName = "B",
                Reminder = false,
                Email = "abc@xyz.com",
                Joined = DateTime.Now
            };
        }
    }
}
