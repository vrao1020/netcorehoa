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
using Microsoft.Extensions.Logging;

namespace HoaWebAPIUnitTests.ControllerTests
{
    public class BoardMeetingControllerShould
    {
        private BoardMeetingsController boardMeetingsController;
        private Mock<IBoardMeetingRepository> boardMeetingRepository;
        private Mock<IUserRepository> userRepository;
        private Mock<IMapper> mapper;
        private Mock<SieveModel> sieveModel;
        private Mock<BoardMeetingInputDto> boardMeetingInputDto;
        private Mock<BoardMeetingUpdateDto> boardMeetingUpdateDto;
        private Mock<JsonPatchDocument<BoardMeetingUpdateDto>> jsonPatchBoardMeetingUpdateDto;
        private Mock<ISortFilterService<BoardMeeting>> sortFilterService;

        public BoardMeetingControllerShould()
        {
            boardMeetingRepository = new Mock<IBoardMeetingRepository>();
            userRepository = new Mock<IUserRepository>();
            mapper = new Mock<IMapper>();
            sieveModel = new Mock<SieveModel>();
            boardMeetingInputDto = new Mock<BoardMeetingInputDto>();
            boardMeetingUpdateDto = new Mock<BoardMeetingUpdateDto>();
            jsonPatchBoardMeetingUpdateDto = new Mock<JsonPatchDocument<BoardMeetingUpdateDto>>();
            sortFilterService = new Mock<ISortFilterService<BoardMeeting>>();

            // Arrange
            boardMeetingRepository.Setup(repo => repo.GetBoardMeetingAsync(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"))).ReturnsAsync(GetTestBoardMeeting());
            boardMeetingRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => true);
            boardMeetingRepository.Setup(repo => repo.BoardMeetingExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            boardMeetingRepository.Setup(repo => repo.GetBoardMeetings()).Returns(() => null);

            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);

            mapper.Setup(map => map.Map<BoardMeetingViewDto>(It.IsAny<BoardMeeting>())).Returns(GetTestBoardMeetingViewDto);
            mapper.Setup(map => map.Map<BoardMeeting>(It.IsAny<BoardMeetingInputDto>())).Returns(GetTestBoardMeeting);
            mapper.Setup(map => map.Map<IEnumerable<BoardMeetingViewDto>>(It.IsAny<IEnumerable<BoardMeeting>>())).Returns(GetTestBoardMeetingsViewDto);
            mapper.Setup(map => map.Map<BoardMeetingUpdateDto>(It.IsAny<BoardMeeting>())).Returns(GetTestBoardMeetingUpdateDto);

            boardMeetingsController = new BoardMeetingsController(boardMeetingRepository.Object, mapper.Object, userRepository.Object,
                sortFilterService.Object);

            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);
        }

        [Fact]
        public async Task GetSingleBoardMeeting_ReturnsNotFound_WhenInvalidGuidUsed()
        {
            // Arrange

            //Act
            var result = await boardMeetingsController.GetBoardMeeting(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1699"));

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task GetSingleBoardMeeting_ReturnsNotNull_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await boardMeetingsController.GetBoardMeeting(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSingleBoardMeeting_ReturnsOkResult_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await boardMeetingsController.GetBoardMeeting(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetSingleBoardMeeting_ReturnsBoardMeetingViewDto_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await boardMeetingsController.GetBoardMeeting(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.IsAssignableFrom<BoardMeetingViewDto>(okResult.Value);
        }


        [Fact]
        public async Task GetSingleBoardMeeting_ReturnsRightBoardMeeting_WhenValidGuidUsed()
        {
            // Arrange
            var guid = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619");
            //Act
            var controllerresult = await boardMeetingsController.GetBoardMeeting(guid);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var model = Assert.IsAssignableFrom<BoardMeetingViewDto>(result.Value);
            Assert.Equal(guid, model.Id);
        }

        [Fact]
        public async Task GetSingleBoardMeeting_WithoutMeetingMinute_ReturnsBoardMeetingViewDto_WithoutFileName()
        {
            // Arrange
            var guid = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619");
            //Act
            var controllerresult = await boardMeetingsController.GetBoardMeeting(guid);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var model = Assert.IsAssignableFrom<BoardMeetingViewDto>(result.Value);
            Assert.Null(model.FileName);
        }

        [Fact]
        public async Task GetSingleBoardMeeting_WithMeetingMinute_ReturnsBoardMeetingViewDto_WithFileName()
        {
            // Arrange
            boardMeetingRepository.Setup(repo => repo.GetBoardMeetingAsync(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"))).ReturnsAsync(GetTestBoardMeetingWithMeetingMinute);
            mapper.Setup(map => map.Map<BoardMeetingViewDto>(It.IsAny<BoardMeeting>())).Returns(GetTestBoardMeetingViewDtoWithFileName);
            var guid = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619");
            //Act
            var controllerresult = await boardMeetingsController.GetBoardMeeting(guid);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var model = Assert.IsAssignableFrom<BoardMeetingViewDto>(result.Value);
            Assert.NotNull(model.FileName);
        }

        [Fact]
        public async Task GetBoardMeetings_ReturnsOkResult()
        {
            // Arrange

            //Act
            var result = await boardMeetingsController.GetBoardMeetings(sieveModel.Object);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetBoardMeetings_Returns_IEnumerableOfBoardMeetingViewDto()
        {
            // Arrange

            //Act
            var controllerresult = await boardMeetingsController.GetBoardMeetings(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;


            //Assert
            Assert.IsAssignableFrom<IEnumerable<BoardMeetingViewDto>>(result.Value);
        }

        [Fact]
        public async Task GetBoardMeetings_Returns_ExactlyTwo_BoardMeetingViewDtos()
        {
            // Arrange

            //Act
            var controllerresult = await boardMeetingsController.GetBoardMeetings(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var boardMeetings = Assert.IsAssignableFrom<IEnumerable<BoardMeetingViewDto>>(result.Value);

            Assert.Equal(2, boardMeetings.Count());
        }

        [Fact]
        public async Task GetBoardMeetings_Returns_ExactlyTwo_BoardMeetingViewDtos_WithFileNames()
        {
            // Arrange         
            mapper.Setup(map => map.Map<IEnumerable<BoardMeetingViewDto>>(It.IsAny<IEnumerable<BoardMeeting>>())).Returns(GetTestBoardMeetingsViewDtoWithFileName);

            //Act
            var controllerresult = await boardMeetingsController.GetBoardMeetings(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var boardMeetings = Assert.IsAssignableFrom<IEnumerable<BoardMeetingViewDto>>(result.Value);

            Assert.Equal(2, boardMeetings.Count());
            Assert.Equal(2, boardMeetings.Select(x => x.FileName).Count());
        }

        [Fact]
        public async Task GetBoardMeetings_Returns_ExactlyOne_BoardMeetingViewDto_WithTitleFilter()
        {
            // Arrange
            sieveModel.Object.Filters = "Title@=*1";
            mapper.Setup(map => map.Map<IEnumerable<BoardMeetingViewDto>>(It.IsAny<IEnumerable<BoardMeeting>>())).Returns(GetTestBoardMeetingsViewDtoSingle);

            //Act
            var controllerresult = await boardMeetingsController.GetBoardMeetings(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var boardMeetings = Assert.IsAssignableFrom<IEnumerable<BoardMeetingViewDto>>(result.Value);

            Assert.Single(boardMeetings);
        }

        [Fact]
        public async Task Add_InvalidBoardMeeting_Returns_BadRequest()
        {
            //Arrange
            BoardMeetingInputDto x = null;

            //Act
            var result = await boardMeetingsController.CreateBoardMeeting(x);

            //Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task Add_InvalidBoardMeeting_WithoutValidUser_Returns_BadRequest()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var result = await boardMeetingsController.CreateBoardMeeting(boardMeetingInputDto.Object);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Add_InvalidBoardMeeting_WithModelErrors_Returns_UnprocessableEntityRequest()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            boardMeetingsController.ModelState.AddModelError("X", "Invalid model state");

            //Act
            var result = await boardMeetingsController.CreateBoardMeeting(boardMeetingInputDto.Object);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Add_ValidBoardMeeting__Returns_CreatedAtRouteResult()
        {
            //Arrange

            //Act
            var result = await boardMeetingsController.CreateBoardMeeting(boardMeetingInputDto.Object);

            //Assert
            Assert.IsType<CreatedAtRouteResult>(result.Result);
        }

        [Fact]
        public async Task Add_ValidBoardMeeting__Returns_BoardMeetingViewDto()
        {
            //Arrange

            //Act            
            var controllerresult = await boardMeetingsController.CreateBoardMeeting(boardMeetingInputDto.Object);
            var result = controllerresult.Result as CreatedAtRouteResult;


            //Assert
            Assert.IsType<BoardMeetingViewDto>(result.Value);
        }

        [Fact]
        public async Task Add_ValidBoardMeeting_WithoutFileName_Returns_BoardMeetingViewDto_WithoutFileName()
        {
            //Arrange

            //Act
            var controllerresult = await boardMeetingsController.CreateBoardMeeting(boardMeetingInputDto.Object);
            var result = controllerresult.Result as CreatedAtRouteResult;

            //Assert
            var boardMeeting = Assert.IsType<BoardMeetingViewDto>(result.Value);

            Assert.Null(boardMeeting.FileName);

        }

        [Fact]
        public async Task Add_ValidBoardMeeting_FileName_Returns_BoardMeetingViewDto_FileName()
        {
            //Arrange
            mapper.Setup(map => map.Map<BoardMeetingViewDto>(It.IsAny<BoardMeeting>())).Returns(GetTestBoardMeetingViewDtoWithFileName);

            //Act
            var controllerresult = await boardMeetingsController.CreateBoardMeeting(boardMeetingInputDto.Object);
            var result = controllerresult.Result as CreatedAtRouteResult;

            //Assert
            var boardMeeting = Assert.IsType<BoardMeetingViewDto>(result.Value);

            Assert.NotNull(boardMeeting.FileName);

        }

        [Fact]
        public async Task Add_ValidBoardMeeting__Returns_CreatedAtRouteResult_At_GetBoardMeeting_Action()
        {
            //Arrange

            //Act
            var controllerresult = await boardMeetingsController.CreateBoardMeeting(boardMeetingInputDto.Object);
            var result = controllerresult.Result as CreatedAtRouteResult;

            //Assert
            Assert.Equal("GetBoardMeeting", result.RouteName);
        }

        [Fact]
        public async Task Checking_Existing_BoardMeetingId__Returns_ObjectResult()
        {
            //Arrange

            //Act
            var result = await boardMeetingsController.BlockBoardMeetingCreation(Guid.NewGuid());

            //Assert
            Assert.IsType<ObjectResult>(result);
        }

        [Fact]
        public async Task Checking_Existing_BoardMeetingId__Returns_409StatusCode()
        {
            //Arrange

            //Act
            var result = await boardMeetingsController.BlockBoardMeetingCreation(Guid.NewGuid()) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        }

        [Fact]
        public async Task Delete_BoardMeeting_ThatDoesNotExist_Returns_NotFound()
        {
            //Arrange
            boardMeetingRepository.Setup(repo => repo.GetBoardMeetingAsync(It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var result = await boardMeetingsController.DeleteBoardMeeting(Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_BoardMeeting_OfDifferentUser_Returns_ForbiddenStatusCode()
        {
            //Arrange
            boardMeetingRepository.Setup(repo => repo.GetBoardMeetingAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestBoardMeeting);
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var result = await boardMeetingsController.DeleteBoardMeeting(Guid.NewGuid()) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Delete_BoardMeeting_Returns_NoContent()
        {
            //Arrange
            boardMeetingRepository.Setup(repo => repo.GetBoardMeetingAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestBoardMeeting);

            //Act
            var result = await boardMeetingsController.DeleteBoardMeeting(Guid.NewGuid());

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_WithInValidBoardMeeting_Returns_UnProcessableEntityResult()
        {
            //Arrange

            //Act
            var result = await boardMeetingsController.UpdateBoardMeeting(Guid.NewGuid(), boardMeetingUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_BoardMeeting_Returns_NoContent()
        {
            //Arrange
            boardMeetingRepository.Setup(repo => repo.GetBoardMeetingAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestBoardMeeting);

            //Act
            var result = await boardMeetingsController.UpdateBoardMeeting(Guid.NewGuid(), boardMeetingUpdateDto.Object);

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_BoardMeetingNotInDatabase_Returns_NotFoundResult()
        {
            //Arrange

            //Act
            var result = await boardMeetingsController.UpdateBoardMeeting(Guid.NewGuid(), boardMeetingUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_DatabaseSaveError_Throws_Exception()
        {
            //Arrange
            boardMeetingRepository.Setup(repo => repo.GetBoardMeetingAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestBoardMeeting);
            boardMeetingRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => false);

            //Act

            //Assert
            await Assert.ThrowsAsync<Exception>(() => boardMeetingsController.UpdateBoardMeeting(Guid.NewGuid(), boardMeetingUpdateDto.Object));
        }

        [Fact]
        public async Task Update_OnPatch_With_DatabaseSaveError_Throws_Exception()
        {
            //Arrange
            boardMeetingRepository.Setup(repo => repo.GetBoardMeetingAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestBoardMeeting);
            boardMeetingRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => false);
            jsonPatchBoardMeetingUpdateDto.Object.Replace(x => x.Title, "ABC");
            JsonPatchDocument<BoardMeetingUpdateDto> test = new JsonPatchDocument<BoardMeetingUpdateDto>();
            test.Replace(x => x.Title, "ABC");

            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(),
                                              It.IsAny<ValidationStateDictionary>(),
                                              It.IsAny<string>(),
                                              It.IsAny<Object>()))
                                              .Throws(new Exception());
            boardMeetingsController.ObjectValidator = objectValidator.Object;

            //Act

            //Assert
            await Assert.ThrowsAsync<Exception>(() => boardMeetingsController.PartialUpdateBoardMeeting(Guid.NewGuid(), test));
        }

        private BoardMeetingUpdateDto GetTestBoardMeetingUpdateDto()
        {
            return new BoardMeetingUpdateDto
            {
                Title = "test title",
                Description = "Test message",
                ScheduledTime = DateTime.Now,
                ScheduledLocation = "a"
            };
        }

        private BoardMeetingInputDto GetBoardMeetingForInput()
        {
            return new BoardMeetingInputDto
            {
                Title = "test title",
                Description = "Test message",
                ScheduledTime = DateTime.Now,
                ScheduledLocation = "a"
            };
        }

        private BoardMeetingInputDto GetBoardMeetingForInputWithFileName()
        {
            return new BoardMeetingInputDto
            {
                Title = "test title",
                Description = "Test message",
                ScheduledTime = DateTime.Now,
                ScheduledLocation = "a",
                MeetingNotes = new MeetingMinuteInputDto
                {
                    FileName = "abc.txt"
                }
            };
        }

        private BoardMeetingViewDto GetTestBoardMeetingViewDto()
        {
            var boardMeetings = new BoardMeetingViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Description = "Test1",
                DaysOld = 0,
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com",
                ScheduledLocation = "a"
            };

            return boardMeetings;
        }

        private IEnumerable<BoardMeetingViewDto> GetTestBoardMeetingsViewDto()
        {
            var boardMeetings = new List<BoardMeetingViewDto>();
            boardMeetings.Add(new BoardMeetingViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Description = "Test1",
                DaysOld = 0,
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com",
                ScheduledLocation = "a",
                OwnerSocialId = "123"
            });
            boardMeetings.Add(new BoardMeetingViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                Title = "Test2",
                Description = "Test2",
                DaysOld = 0,
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                OwnerName = "XYZ",
                OwnerEmail = "Test@test.com",
                ScheduledLocation = "a",
                OwnerSocialId = "123"
            });

            return boardMeetings;
        }

        private BoardMeetingViewDto GetTestBoardMeetingViewDtoWithFileName()
        {
            var boardMeetings = new BoardMeetingViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Description = "Test1",
                DaysOld = 0,
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com",
                ScheduledLocation = "a",
                FileName = "abc.txt",
                OwnerSocialId = "123"
            };

            return boardMeetings;
        }

        private IEnumerable<BoardMeetingViewDto> GetTestBoardMeetingsViewDtoWithFileName()
        {
            var boardMeetings = new List<BoardMeetingViewDto>();
            boardMeetings.Add(new BoardMeetingViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Description = "Test1",
                DaysOld = 0,
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com",
                ScheduledLocation = "a",
                FileName = "abc.txt",
                OwnerSocialId = "123"
            });
            boardMeetings.Add(new BoardMeetingViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                Title = "Test2",
                Description = "Test2",
                DaysOld = 0,
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                OwnerName = "XYZ",
                OwnerEmail = "Test@test.com",
                ScheduledLocation = "a",
                FileName = "abc.txt",
                OwnerSocialId = "123"
            });

            return boardMeetings;
        }

        private IEnumerable<BoardMeetingViewDto> GetTestBoardMeetingsViewDtoSingle()
        {
            var boardMeetings = new List<BoardMeetingViewDto>();
            boardMeetings.Add(new BoardMeetingViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Description = "Test1",
                DaysOld = 0,
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com",
                ScheduledLocation = "a",
                FileName = "abc.txt",
                OwnerSocialId = "123"
            });
            return boardMeetings;
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

        private BoardMeeting GetTestBoardMeetingWithMeetingMinute()
        {
            var boardMeetings = new BoardMeeting()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Description = "Test1",
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9"),
                ScheduledLocation = "a",
                MeetingNotes = new MeetingMinute
                {
                    BoardMeetingId = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                    FileName = "Abc.txt",
                    Id = Guid.NewGuid(),
                    Created = DateTime.Now,
                    UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
                }
            };

            return boardMeetings;
        }

        private IEnumerable<BoardMeeting> GetTestBoardMeetingsWithFileName()
        {
            var boardMeetings = new List<BoardMeeting>();
            boardMeetings.Add(new BoardMeeting()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Description = "Test1",
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9"),
                ScheduledLocation = "a",
                MeetingNotes = new MeetingMinute
                {
                    BoardMeetingId = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                    FileName = "Abc.txt",
                    Id = Guid.NewGuid(),
                    Created = DateTime.Now,
                    UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
                }
            });
            boardMeetings.Add(new BoardMeeting()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                Title = "Test2",
                Description = "Test2",
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9"),
                ScheduledLocation = "a",
                MeetingNotes = new MeetingMinute
                {
                    BoardMeetingId = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                    FileName = "Abc.txt",
                    Id = Guid.NewGuid(),
                    Created = DateTime.Now,
                    UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
                }
            });
            return boardMeetings.AsEnumerable();
        }

        private IEnumerable<BoardMeeting> GetTestBoardMeetings()
        {
            var boardMeetings = new List<BoardMeeting>();
            boardMeetings.Add(new BoardMeeting()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Description = "Test1",
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9"),
                ScheduledLocation = "a"
            });
            boardMeetings.Add(new BoardMeeting()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                Title = "Test2",
                Description = "Test2",
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9"),
                ScheduledLocation = "a"
            });
            return boardMeetings.AsEnumerable();
        }

        private IEnumerable<BoardMeeting> GetTestBoardMeetingsSingleBoardMeeting()
        {
            var boardMeetings = new List<BoardMeeting>();
            boardMeetings.Add(new BoardMeeting()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Description = "Test1",
                Created = DateTime.Now,
                ScheduledTime = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9"),
                ScheduledLocation = "a"
            });
            return boardMeetings.AsEnumerable();
        }

        private User GetTestUser()
        {
            return new User
            {
                Id = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9"),
                SocialId = "|100843189528048286886",
                FirstName = "A",
                LastName = "B",
                Reminder = false,
                Email = "abc@xyz.com",
                Joined = DateTime.Now
            };
        }
    }
}
