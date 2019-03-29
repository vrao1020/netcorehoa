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
    public class UserControllerShould
    {

        private UsersController usersController;
        private Mock<IUserRepository> userRepository;
        private Mock<IMapper> mapper;
        private Mock<SieveModel> sieveModel;
        private Mock<UserInputDto> userInputDto;
        private Mock<UserUpdateDto> userUpdateDto;
        private Mock<JsonPatchDocument<UserUpdateDto>> jsonPatchUserUpdateDto;
        private Mock<ISortFilterService<User>> sortFilterService;

        public UserControllerShould()
        {
            userRepository = new Mock<IUserRepository>();
            mapper = new Mock<IMapper>();
            sieveModel = new Mock<SieveModel>();
            userInputDto = new Mock<UserInputDto>();
            userUpdateDto = new Mock<UserUpdateDto>();
            jsonPatchUserUpdateDto = new Mock<JsonPatchDocument<UserUpdateDto>>();
            sortFilterService = new Mock<ISortFilterService<User>>();

            // Arrange
            userRepository.Setup(repo => repo.GetUserAsync(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"))).ReturnsAsync(GetTestUser());
            userRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => true);
            userRepository.Setup(repo => repo.UserExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            userRepository.Setup(repo => repo.GetUsers()).Returns(() => null);
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);

            mapper.Setup(map => map.Map<UserViewDto>(It.IsAny<User>())).Returns(GetTestUserViewDto);
            mapper.Setup(map => map.Map<User>(It.IsAny<UserInputDto>())).Returns(GetTestUser);
            mapper.Setup(map => map.Map<IEnumerable<UserViewDto>>(It.IsAny<IEnumerable<User>>())).Returns(GetTestUsersViewDto);
            mapper.Setup(map => map.Map<UserUpdateDto>(It.IsAny<User>())).Returns(GetTestUserUpdateDto);

            usersController = new UsersController(mapper.Object, userRepository.Object, sortFilterService.Object);

            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);
        }

        [Fact]
        public async Task GetSingleUser_ReturnsNotFound_WhenInvalidGuidUsed()
        {
            // Arrange

            //Act
            var result = await usersController.GetUser(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1699"));

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task GetSingleUser_ReturnsNotNull_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await usersController.GetUser(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSingleUser_ReturnsOkResult_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await usersController.GetUser(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetSingleUser_ReturnsUserViewDto_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await usersController.GetUser(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.IsAssignableFrom<UserViewDto>(okResult.Value);
        }


        [Fact]
        public async Task GetSingleUser_ReturnsRightUser_WhenValidGuidUsed()
        {
            // Arrange
            var guid = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619");

            //Act            
            var controllerresult = await usersController.GetUser(guid);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var model = Assert.IsAssignableFrom<UserViewDto>(result.Value);
            Assert.Equal("test title", model.FirstName);
        }

        [Fact]
        public async Task GetUsers_ReturnsOkResult()
        {
            // Arrange

            //Act
            var result = await usersController.GetUsers(sieveModel.Object);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUsers_Returns_IEnumerableOfUserViewDto()
        {
            // Arrange

            //Act            
            var controllerresult = await usersController.GetUsers(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            Assert.IsAssignableFrom<IEnumerable<UserViewDto>>(result.Value);
        }

        [Fact]
        public async Task GetUsers_Returns_ExactlyTwo_UserViewDtos()
        {
            // Arrange

            //Act
            var controllerresult = await usersController.GetUsers(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var users = Assert.IsAssignableFrom<IEnumerable<UserViewDto>>(result.Value);

            Assert.Equal(2, users.Count());
        }

        [Fact]
        public async Task GetUsers_Returns_ExactlyOne_UserViewDto_WithTitleFilter()
        {
            // Arrange
            sieveModel.Object.Filters = "Title@=*1";
            mapper.Setup(map => map.Map<IEnumerable<UserViewDto>>(It.IsAny<IEnumerable<User>>())).Returns(GetTestUsersViewDtoSingle);

            //Act
            var controllerresult = await usersController.GetUsers(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var users = Assert.IsAssignableFrom<IEnumerable<UserViewDto>>(result.Value);

            Assert.Single(users);
        }

        [Fact]
        public async Task Add_InvalidUser_Returns_BadRequest()
        {
            //Arrange
            UserInputDto x = null;

            //Act
            var result = await usersController.CreateUser(x);

            //Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task Add_ExistingUser_Returns_BadRequest()
        {
            //Arrange

            //Act
            var result = await usersController.CreateUser(userInputDto.Object);

            //Assert
            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        [Fact]
        public async Task Add_InvalidUser_WithModelErrors_Returns_UnprocessableEntityRequest()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            usersController.ModelState.AddModelError("X", "Invalid model state");

            //Act
            var result = await usersController.CreateUser(userInputDto.Object);

            //Assert
            Assert.IsType<CreatedAtRouteResult>(result.Result);
        }

        [Fact]
        public async Task Add_ValidUser__Returns_CreatedAtRouteResult()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var result = await usersController.CreateUser(userInputDto.Object);

            //Assert
            Assert.IsType<CreatedAtRouteResult>(result.Result);
        }

        [Fact]
        public async Task Add_ValidUser__Returns_UserViewDto()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act            
            var controllerresult = await usersController.CreateUser(userInputDto.Object);
            var result = controllerresult.Result as CreatedAtRouteResult;

            //Assert
            Assert.IsType<UserViewDto>(result.Value);
        }

        [Fact]
        public async Task Add_ValidUser__Returns_CreatedAtRouteResult_At_GetUser_Action()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var controllerresult = await usersController.CreateUser(userInputDto.Object);
            var result = controllerresult.Result as CreatedAtRouteResult;

            //Assert
            Assert.Equal("GetUser", result.RouteName);
        }

        [Fact]
        public async Task Checking_Existing_UserId__Returns_ObjectResult()
        {
            //Arrange

            //Act
            var result = await usersController.BlockUserCreation(Guid.NewGuid());

            //Assert
            Assert.IsType<ObjectResult>(result);
        }

        [Fact]
        public async Task Checking_Existing_UserId__Returns_409StatusCode()
        {
            //Arrange

            //Act
            var result = await usersController.BlockUserCreation(Guid.NewGuid()) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        }

        [Fact]
        public async Task Delete_User_ThatDoesNotExist_Returns_NotFound()
        {
            //Arrange
            userRepository.Setup(repo => repo.GetUserAsync(It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var result = await usersController.DeleteUser(Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_User_OfDifferentUser_Returns_ForbiddenStatusCode()
        {
            //Arrange
            userRepository.Setup(repo => repo.GetUserAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestUserNotOwner);

            //Act
            var result = await usersController.DeleteUser(Guid.NewGuid()) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Delete_User_Returns_NoContent()
        {
            //Arrange
            userRepository.Setup(repo => repo.GetUserAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestUser);

            //Act
            var result = await usersController.DeleteUser(Guid.NewGuid());

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_ExistingUser_Returns_NoContent()
        {
            //Arrange
            userRepository.Setup(repo => repo.GetUserAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestUser);

            //Act
            var result = await usersController.UpdateUser(userUpdateDto.Object);

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Upsert_WithPut_Returns_CreatedAtRouteResult()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            mapper.Setup(map => map.Map<User>(It.IsAny<UserUpdateDto>())).Returns(GetTestUser);

            //Act
            var result = await usersController.UpdateUser(userUpdateDto.Object);

            //Assert
            Assert.IsType<CreatedAtRouteResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_ASAdmin_With_DatabaseSaveError_Throws_Exception()
        {
            //Arrange
            userRepository.Setup(repo => repo.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);
            userRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => false);

            //Act

            //Assert
            await Assert.ThrowsAsync<Exception>(() => usersController.UpdateUser(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"), userUpdateDto.Object));
        }

        [Fact]
        public async Task Update_OnPut_ASAdmin_ForDifferentUser_Returns_Forbidden()
        {
            //Arrange
            userRepository.Setup(repo => repo.GetUserAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestUser);

            //Act
            var result = await usersController.UpdateUser(Guid.NewGuid(), userUpdateDto.Object) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Update_OnPut_ASAdmin_With_User_Returns_NoContent()
        {
            //Arrange
            userRepository.Setup(repo => repo.GetUserAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestUser);

            //Act
            var result = await usersController.UpdateUser(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"), userUpdateDto.Object);

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_ASAdmin_With_UserNotInDatabase_Returns_NotFoundResult()
        {
            //Arrange
            userRepository.Setup(repo => repo.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var result = await usersController.UpdateUser(Guid.NewGuid(), userUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_DatabaseSaveError_Throws_Exception()
        {
            //Arrange
            userRepository.Setup(repo => repo.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);
            userRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => false);

            //Act

            //Assert
            await Assert.ThrowsAsync<Exception>(() => usersController.UpdateUser(userUpdateDto.Object));
        }

        [Fact]
        public async Task Update_OnPatch_With_DatabaseSaveError_Throws_Exception()
        {
            //Arrange
            userRepository.Setup(repo => repo.GetUserAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestUser);
            userRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => false);
            jsonPatchUserUpdateDto.Object.Replace(x => x.FirstName, "ABC");
            JsonPatchDocument<UserUpdateDto> test = new JsonPatchDocument<UserUpdateDto>();
            test.Replace(x => x.FirstName, "ABC");

            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(),
                                              It.IsAny<ValidationStateDictionary>(),
                                              It.IsAny<string>(),
                                              It.IsAny<Object>()))
                                              .Throws(new Exception());
            usersController.ObjectValidator = objectValidator.Object;

            //Act

            //Assert
            await Assert.ThrowsAsync<Exception>(() => usersController.PartialUpdateUser(test));
        }

        private UserUpdateDto GetTestUserUpdateDto()
        {
            return new UserUpdateDto
            {
                FirstName = "test title",
                LastName = "Test message",
                Reminder = false,
                Email = "abc@xyz.com"
            };
        }

        private UserInputDto GetUserForInput()
        {
            return new UserInputDto
            {
                FirstName = "test title",
                LastName = "Test message",
                Reminder = false,
                Email = "abc@xyz.com"
            };
        }

        private UserViewDto GetTestUserViewDto()
        {
            var users = new UserViewDto()
            {
                FullName = "ABC",
                FirstName = "test title",
                LastName = "Test message",
                Reminder = false,
                Email = "abc@xyz.com"
            };

            return users;
        }

        private IEnumerable<UserViewDto> GetTestUsersViewDto()
        {
            var users = new List<UserViewDto>();
            users.Add(new UserViewDto()
            {
                FullName = "ABC",
                FirstName = "test title",
                LastName = "Test message",
                Reminder = false,
                Email = "abc@xyz.com",
                Active = false
            });
            users.Add(new UserViewDto()
            {
                FullName = "ABCD",
                FirstName = "test title",
                LastName = "Test message",
                Reminder = false,
                Email = "abc@xyz.com",
                Active = false
            });

            return users;
        }

        private IEnumerable<UserViewDto> GetTestUsersViewDtoSingle()
        {
            var users = new List<UserViewDto>();
            users.Add(new UserViewDto()
            {
                FullName = "ABC",
                FirstName = "test title",
                LastName = "Test message",
                Reminder = false,
                Email = "abc@xyz.com",
                Active = false
            });
            return users;
        }

        private User GetTestUser()
        {
            var users = new User()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                SocialId = "Test1",
                FirstName = "test title",
                LastName = "Test message",
                Reminder = false,
                Email = "abc@xyz.com",
                Active = false,
                Joined = DateTime.Now
            };

            return users;
        }

        private User GetTestUserNotOwner()
        {
            var users = new User()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                SocialId = "Test1",
                FirstName = "test title",
                LastName = "Test message",
                Reminder = false,
                Email = "abc@xyz.com",
                Active = false,
                Joined = DateTime.Now
            };

            return users;
        }

        private IEnumerable<User> GetTestUsers()
        {
            var users = new List<User>();
            users.Add(new User()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                SocialId = "Test1",
                FirstName = "test title",
                LastName = "Test message",
                Reminder = false,
                Email = "abc@xyz.com",
                Active = false,
                Joined = DateTime.Now
            });
            users.Add(new User()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                SocialId = "Test1",
                FirstName = "test title",
                LastName = "Test message",
                Reminder = false,
                Email = "abc@xyz.com",
                Active = false,
                Joined = DateTime.Now
            });
            return users.AsEnumerable();
        }

        private IEnumerable<User> GetTestUsersSingleUser()
        {
            var users = new List<User>();
            users.Add(new User()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                SocialId = "Test1",
                FirstName = "test title",
                LastName = "Test message",
                Reminder = false,
                Email = "abc@xyz.com",
                Active = false,
                Joined = DateTime.Now
            });
            return users.AsEnumerable();
        }
    }
}
