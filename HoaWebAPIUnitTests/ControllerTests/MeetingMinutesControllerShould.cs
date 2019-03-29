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
using Moq;
using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HoaWebAPIUnitTests.ControllerTests
{
    public class MeetingMinutesControllerShould
    {

        private MeetingMinutesController meetingMinutesController;
        private Mock<IMeetingMinuteRepository> meetingMinuteRepository;
        private Mock<IBoardMeetingRepository> boardMeetingRepository;
        private Mock<IUserRepository> userRepository;
        private Mock<IMapper> mapper;
        private Mock<SieveModel> sieveModel;
        private Mock<MeetingMinuteInputDto> meetingMinuteInputDto;
        private Mock<MeetingMinuteUpdateDto> meetingMinuteUpdateDto;
        private Mock<JsonPatchDocument<MeetingMinuteUpdateDto>> jsonPatchMeetingMinuteUpdateDto;
        private Mock<ISortFilterService<MeetingMinute>> sortFilterService;

        public MeetingMinutesControllerShould()
        {
            meetingMinuteRepository = new Mock<IMeetingMinuteRepository>();
            userRepository = new Mock<IUserRepository>();
            boardMeetingRepository = new Mock<IBoardMeetingRepository>();
            mapper = new Mock<IMapper>();
            sieveModel = new Mock<SieveModel>();
            meetingMinuteInputDto = new Mock<MeetingMinuteInputDto>();
            meetingMinuteUpdateDto = new Mock<MeetingMinuteUpdateDto>();
            jsonPatchMeetingMinuteUpdateDto = new Mock<JsonPatchDocument<MeetingMinuteUpdateDto>>();
            sortFilterService = new Mock<ISortFilterService<MeetingMinute>>();

            // Arrange
            boardMeetingRepository.Setup(repo => repo.GetBoardMeetingAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestBoardMeeting);
            boardMeetingRepository.Setup(repo => repo.BoardMeetingExistsAsync(It.IsAny<Guid>())).ReturnsAsync(() => true);

            meetingMinuteRepository.Setup(repo => repo.GetMeetingMinuteAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(GetTestMeetingMinute());
            meetingMinuteRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => true);
            meetingMinuteRepository.Setup(repo => repo.MeetingMinuteExistsAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);
            meetingMinuteRepository.Setup(repo => repo.GetMeetingMinutes()).Returns(() => null);

            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);

            mapper.Setup(map => map.Map<MeetingMinuteViewDto>(It.IsAny<MeetingMinute>())).Returns(GetTestMeetingMinuteViewDto);
            mapper.Setup(map => map.Map<MeetingMinute>(It.IsAny<MeetingMinuteInputDto>())).Returns(GetTestMeetingMinute);
            mapper.Setup(map => map.Map<IEnumerable<MeetingMinuteViewDto>>(It.IsAny<IEnumerable<MeetingMinute>>())).Returns(GetTestMeetingMinutesViewDto);
            mapper.Setup(map => map.Map<MeetingMinuteUpdateDto>(It.IsAny<MeetingMinute>())).Returns(GetTestMeetingMinuteUpdateDto);

            meetingMinutesController = new MeetingMinutesController(meetingMinuteRepository.Object, mapper.Object, userRepository.Object, boardMeetingRepository.Object,
                sortFilterService.Object);

            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);
        }

        [Fact]
        public async Task GetSingleMeetingMinute_ReturnsNotFound_WhenBoardMeetingNotExists()
        {
            // Arrange
            boardMeetingRepository.Setup(repo => repo.BoardMeetingExistsAsync(It.IsAny<Guid>())).ReturnsAsync(() => false);

            //Act
            var result = await meetingMinutesController.GetMeetingMinute(Guid.NewGuid(), new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1699"));

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateMeetingMinute_ReturnsNotFound_WhenBoardMeetingNotExists()
        {
            // Arrange
            boardMeetingRepository.Setup(repo => repo.GetBoardMeetingAsync(It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var result = await meetingMinutesController.CreateMeetingMinute(Guid.NewGuid(), meetingMinuteInputDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateMeetingMinute_ReturnsNotFound_WhenBoardMeetingNotExists()
        {
            // Arrange
            boardMeetingRepository.Setup(repo => repo.BoardMeetingExistsAsync(It.IsAny<Guid>())).ReturnsAsync(() => false);

            //Act
            var result = await meetingMinutesController.UpdateMeetingMinute(Guid.NewGuid(), Guid.NewGuid(), meetingMinuteUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PartialUpdateMeetingMinute_ReturnsNotFound_WhenBoardMeetingNotExists()
        {
            // Arrange
            boardMeetingRepository.Setup(repo => repo.BoardMeetingExistsAsync(It.IsAny<Guid>())).ReturnsAsync(() => false);

            //Act
            var result = await meetingMinutesController.PartialUpdateMeetingMinute(Guid.NewGuid(), Guid.NewGuid(), jsonPatchMeetingMinuteUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteMeetingMinute_ReturnsNotFound_WhenBoardMeetingNotExists()
        {
            // Arrange
            boardMeetingRepository.Setup(repo => repo.BoardMeetingExistsAsync(It.IsAny<Guid>())).ReturnsAsync(() => false);

            //Act
            var result = await meetingMinutesController.DeleteMeetingMinute(Guid.NewGuid(), new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1699"));

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetSingleMeetingMinute_ReturnsNotFound_WhenInvalidGuidUsed()
        {
            // Arrange
            meetingMinuteRepository.Setup(repo => repo.GetMeetingMinuteAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var result = await meetingMinutesController.GetMeetingMinute(Guid.NewGuid(), new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1699"));

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task GetSingleMeetingMinute_ReturnsNotNull_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await meetingMinutesController.GetMeetingMinute(Guid.NewGuid(), new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSingleMeetingMinute_ReturnsOkResult_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await meetingMinutesController.GetMeetingMinute(Guid.NewGuid(), new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetSingleMeetingMinute_ReturnsMeetingMinuteViewDto_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await meetingMinutesController.GetMeetingMinute(Guid.NewGuid(), new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.IsAssignableFrom<MeetingMinuteViewDto>(okResult.Value);
        }


        [Fact]
        public async Task GetSingleMeetingMinute_ReturnsRightMeetingMinute_WhenValidGuidUsed()
        {
            // Arrange
            var guid = new Guid("e77551ba-78e3-4a36-9754-3ea5f12e1619");
            meetingMinuteRepository.Setup(repo => repo.GetMeetingMinuteAsync(It.IsAny<Guid>(), new Guid("e77551ba-78e3-4a36-9754-3ea5f12e1619"))).ReturnsAsync(GetTestMeetingMinute());

            //Act            
            var controllerresult = await meetingMinutesController.GetMeetingMinute(Guid.NewGuid(), guid);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var model = Assert.IsAssignableFrom<MeetingMinuteViewDto>(result.Value);
            Assert.Equal(guid, model.Id);
        }

        [Fact]
        public async Task GetMeetingMinutes_ReturnsOkResult()
        {
            // Arrange

            //Act
            var result = await meetingMinutesController.GetAllMeetingMinutes(sieveModel.Object);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetMeetingMinutes_Returns_IEnumerableOfMeetingMinuteViewDto()
        {
            // Arrange

            //Act
            var controllerresult = await meetingMinutesController.GetAllMeetingMinutes(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            Assert.IsAssignableFrom<IEnumerable<MeetingMinuteViewDto>>(result.Value);
        }

        [Fact]
        public async Task GetMeetingMinutes_Returns_ExactlyTwo_MeetingMinuteViewDtos()
        {
            // Arrange

            //Act
            var controllerresult = await meetingMinutesController.GetAllMeetingMinutes(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var meetingMinutes = Assert.IsAssignableFrom<IEnumerable<MeetingMinuteViewDto>>(result.Value);

            Assert.Equal(2, meetingMinutes.Count());
        }

        [Fact]
        public async Task GetMeetingMinutes_Returns_ExactlyOne_MeetingMinuteViewDto_WithTitleFilter()
        {
            // Arrange
            sieveModel.Object.Filters = "Title@=*1";
            mapper.Setup(map => map.Map<IEnumerable<MeetingMinuteViewDto>>(It.IsAny<IEnumerable<MeetingMinute>>())).Returns(GetTestMeetingMinutesViewDtoSingle);

            //Act
            var controllerresult = await meetingMinutesController.GetAllMeetingMinutes(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var meetingMinutes = Assert.IsAssignableFrom<IEnumerable<MeetingMinuteViewDto>>(result.Value);

            Assert.Single(meetingMinutes);
        }

        [Fact]
        public async Task Add_InvalidMeetingMinute_Returns_BadRequest()
        {
            //Arrange
            MeetingMinuteInputDto x = null;

            //Act
            var result = await meetingMinutesController.CreateMeetingMinute(Guid.NewGuid(), x);

            //Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task Add_InvalidMeetingMinute_WithoutValidUser_Returns_BadRequest()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var result = await meetingMinutesController.CreateMeetingMinute(Guid.NewGuid(), meetingMinuteInputDto.Object);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Add_InvalidMeetingMinute_WithModelErrors_Returns_UnprocessableEntityRequest()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            meetingMinutesController.ModelState.AddModelError("X", "Invalid model state");

            //Act
            var result = await meetingMinutesController.CreateMeetingMinute(Guid.NewGuid(), meetingMinuteInputDto.Object);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Add_ValidMeetingMinute__Returns_CreatedAtRouteResult()
        {
            //Arrange

            //Act
            var result = await meetingMinutesController.CreateMeetingMinute(Guid.NewGuid(), meetingMinuteInputDto.Object);

            //Assert
            Assert.IsType<CreatedAtRouteResult>(result.Result);
        }

        [Fact]
        public async Task Add_ValidMeetingMinute__Returns_MeetingMinuteViewDto()
        {
            //Arrange

            //Act
            var controllerresult = await meetingMinutesController.CreateMeetingMinute(Guid.NewGuid(), meetingMinuteInputDto.Object);
            var result = controllerresult.Result as CreatedAtRouteResult;

            //Assert
            Assert.IsType<MeetingMinuteViewDto>(result.Value);
        }

        [Fact]
        public async Task Add_ValidMeetingMinute__Returns_CreatedAtRouteResult_At_GetMeetingMinute_Action()
        {
            //Arrange

            //Act
            var controllerresult = await meetingMinutesController.CreateMeetingMinute(Guid.NewGuid(), meetingMinuteInputDto.Object);
            var result = controllerresult.Result as CreatedAtRouteResult;

            //Assert
            Assert.Equal("GetMeetingMinute", result.RouteName);
        }

        [Fact]
        public async Task Checking_Existing_MeetingMinuteId__Returns_ObjectResult()
        {
            //Arrange

            //Act
            var result = await meetingMinutesController.BlockMeetingMinuteCreation(Guid.NewGuid(), Guid.NewGuid());

            //Assert
            Assert.IsType<ObjectResult>(result);
        }

        [Fact]
        public async Task Checking_Existing_MeetingMinuteId__Returns_409StatusCode()
        {
            //Arrange

            //Act
            var result = await meetingMinutesController.BlockMeetingMinuteCreation(Guid.NewGuid(), Guid.NewGuid()) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        }

        [Fact]
        public async Task Delete_MeetingMinute_ThatDoesNotExist_Returns_NotFound()
        {
            //Arrange
            meetingMinuteRepository.Setup(repo => repo.GetMeetingMinuteAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var result = await meetingMinutesController.DeleteMeetingMinute(Guid.NewGuid(), Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_MeetingMinute_OfDifferentUser_Returns_ForbiddenStatusCode()
        {
            //Arrange
            meetingMinuteRepository.Setup(repo => repo.GetMeetingMinuteAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(GetTestMeetingMinute);
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var result = await meetingMinutesController.DeleteMeetingMinute(Guid.NewGuid(), Guid.NewGuid()) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Delete_MeetingMinute_Returns_NoContent()
        {
            //Arrange
            meetingMinuteRepository.Setup(repo => repo.GetMeetingMinuteAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(GetTestMeetingMinute);

            //Act
            var result = await meetingMinutesController.DeleteMeetingMinute(Guid.NewGuid(), Guid.NewGuid());

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_WithInValidMeetingMinute_Returns_UnProcessableEntityResult()
        {
            //Arrange
            meetingMinuteRepository.Setup(repo => repo.GetMeetingMinuteAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var result = await meetingMinutesController.UpdateMeetingMinute(Guid.NewGuid(), Guid.NewGuid(), meetingMinuteUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_MeetingMinute_Returns_NoContent()
        {
            //Arrange
            meetingMinuteRepository.Setup(repo => repo.GetMeetingMinuteAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(GetTestMeetingMinute);

            //Act
            var result = await meetingMinutesController.UpdateMeetingMinute(Guid.NewGuid(), Guid.NewGuid(), meetingMinuteUpdateDto.Object);

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_MeetingMinuteNotInDatabase_Returns_NotFoundResult()
        {
            //Arrange
            meetingMinuteRepository.Setup(repo => repo.GetMeetingMinuteAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var result = await meetingMinutesController.UpdateMeetingMinute(Guid.NewGuid(), Guid.NewGuid(), meetingMinuteUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_DatabaseSaveError_Throws_Exception()
        {
            //Arrange
            meetingMinuteRepository.Setup(repo => repo.GetMeetingMinuteAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(GetTestMeetingMinute);
            meetingMinuteRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => false);

            //Act

            //Assert
            await Assert.ThrowsAsync<Exception>(() => meetingMinutesController.UpdateMeetingMinute(Guid.NewGuid(), Guid.NewGuid(), meetingMinuteUpdateDto.Object));
        }

        [Fact]
        public async Task Update_OnPatch_With_DatabaseSaveError_Throws_Exception()
        {
            //Arrange
            meetingMinuteRepository.Setup(repo => repo.GetMeetingMinuteAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(GetTestMeetingMinute);
            meetingMinuteRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => false);
            jsonPatchMeetingMinuteUpdateDto.Object.Replace(x => x.FileName, "ABC.txt");
            JsonPatchDocument<MeetingMinuteUpdateDto> test = new JsonPatchDocument<MeetingMinuteUpdateDto>();
            test.Replace(x => x.FileName, "ABC.txt");

            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(),
                                              It.IsAny<ValidationStateDictionary>(),
                                              It.IsAny<string>(),
                                              It.IsAny<Object>()))
                                              .Throws(new Exception());
            meetingMinutesController.ObjectValidator = objectValidator.Object;

            //Act

            //Assert
            await Assert.ThrowsAsync<Exception>(() => meetingMinutesController.PartialUpdateMeetingMinute(Guid.NewGuid(), Guid.NewGuid(), test));
        }

        private MeetingMinuteUpdateDto GetTestMeetingMinuteUpdateDto()
        {
            return new MeetingMinuteUpdateDto
            {
                FileName = "abc.txt"
            };
        }

        private MeetingMinuteInputDto GetMeetingMinuteForInput()
        {
            return new MeetingMinuteInputDto
            {
                FileName = "abc.txt"
            };
        }

        private MeetingMinuteViewDto GetTestMeetingMinuteViewDto()
        {
            var meetingMinutes = new MeetingMinuteViewDto()
            {
                Id = new Guid("e77551ba-78e3-4a36-9754-3ea5f12e1619"),
                FileName = "abc.txt",
                DaysOld = 0,
                Created = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com"
            };

            return meetingMinutes;
        }

        private IEnumerable<MeetingMinuteViewDto> GetTestMeetingMinutesViewDto()
        {
            var meetingMinutes = new List<MeetingMinuteViewDto>();
            meetingMinutes.Add(new MeetingMinuteViewDto()
            {
                Id = new Guid("e77551ba-78e3-4a36-9754-3ea5f12e1619"),
                FileName = "abc.txt",
                DaysOld = 0,
                Created = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com"
            });
            meetingMinutes.Add(new MeetingMinuteViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                FileName = "abc.txt",
                DaysOld = 0,
                Created = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com"
            });

            return meetingMinutes;
        }

        private IEnumerable<MeetingMinuteViewDto> GetTestMeetingMinutesViewDtoSingle()
        {
            var meetingMinutes = new List<MeetingMinuteViewDto>();
            meetingMinutes.Add(new MeetingMinuteViewDto()
            {
                Id = new Guid("e77551ba-78e3-4a36-9754-3ea5f12e1619"),
                FileName = "abc.txt",
                DaysOld = 0,
                Created = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com"
            });
            return meetingMinutes;
        }

        private MeetingMinute GetTestMeetingMinute()
        {
            var meetingMinutes = new MeetingMinute()
            {
                Id = new Guid("e77551ba-78e3-4a36-9754-3ea5f12e1619"),
                FileName = "abc.txt",
                Created = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            };

            return meetingMinutes;
        }

        private IEnumerable<MeetingMinute> GetTestMeetingMinutes()
        {
            var meetingMinutes = new List<MeetingMinute>();
            meetingMinutes.Add(new MeetingMinute()
            {
                Id = new Guid("e77551ba-78e3-4a36-9754-3ea5f12e1619"),
                FileName = "abc.txt",
                Created = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            });
            meetingMinutes.Add(new MeetingMinute()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                FileName = "abc.txt",
                Created = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            });
            return meetingMinutes.AsEnumerable();
        }

        private IEnumerable<MeetingMinute> GetTestMeetingMinutesSingleMeetingMinute()
        {
            var meetingMinutes = new List<MeetingMinute>();
            meetingMinutes.Add(new MeetingMinute()
            {
                Id = new Guid("e77551ba-78e3-4a36-9754-3ea5f12e1619"),
                FileName = "abc.txt",
                Created = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            });
            return meetingMinutes.AsEnumerable();
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

        private BoardMeeting GetTestBoardMeeting()
        {
            var boardMeetings = new BoardMeeting()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Description = "Test1",
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9"),
                ScheduledLocation = "a"
            };

            return boardMeetings;
        }
    }
}
