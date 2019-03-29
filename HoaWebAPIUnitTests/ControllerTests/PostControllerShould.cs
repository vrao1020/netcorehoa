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
    public class PostControllerShould
    {
        private PostsController postsController;
        private Mock<IPostRepository> postRepository;
        private Mock<IUserRepository> userRepository;
        private Mock<IMapper> mapper;
        private Mock<SieveModel> sieveModel;
        private Mock<PostInputDto> postInputDto;
        private Mock<PostUpdateDto> postUpdateDto;
        private Mock<JsonPatchDocument<PostUpdateDto>> jsonPatchPostUpdateDto;
        private Mock<ISortFilterService<Post>> sortFilterService;

        public PostControllerShould()
        {
            postRepository = new Mock<IPostRepository>();
            userRepository = new Mock<IUserRepository>();
            mapper = new Mock<IMapper>();
            sieveModel = new Mock<SieveModel>();
            postInputDto = new Mock<PostInputDto>();
            postUpdateDto = new Mock<PostUpdateDto>();
            jsonPatchPostUpdateDto = new Mock<JsonPatchDocument<PostUpdateDto>>();
            sortFilterService = new Mock<ISortFilterService<Post>>();

            // Arrange
            postRepository.Setup(repo => repo.GetPostAsync(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"))).ReturnsAsync(GetTestPost());
            postRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => true);
            postRepository.Setup(repo => repo.PostExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            postRepository.Setup(repo => repo.GetPosts()).Returns(() => null);

            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);

            mapper.Setup(map => map.Map<PostViewDto>(It.IsAny<Post>())).Returns(GetTestPostViewDto);
            mapper.Setup(map => map.Map<Post>(It.IsAny<PostInputDto>())).Returns(GetTestPost);
            mapper.Setup(map => map.Map<IEnumerable<PostViewDto>>(It.IsAny<IEnumerable<Post>>())).Returns(GetTestPostsViewDto);
            mapper.Setup(map => map.Map<PostUpdateDto>(It.IsAny<Post>())).Returns(GetTestPostUpdateDto);

            postsController = new PostsController(postRepository.Object, mapper.Object, userRepository.Object, sortFilterService.Object);

            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);
        }

        [Fact]
        public async Task GetSinglePost_ReturnsNotFound_WhenInvalidGuidUsed()
        {
            // Arrange

            //Act
            var result = await postsController.GetPost(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1699"));

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task GetSinglePost_ReturnsNotNull_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await postsController.GetPost(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSinglePost_ReturnsOkResult_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await postsController.GetPost(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetSinglePost_ReturnsPostViewDto_WhenValidGuidUsed()
        {
            // Arrange

            //Act
            var result = await postsController.GetPost(new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"));

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
            Assert.IsAssignableFrom<PostViewDto>(okResult.Value);
        }


        [Fact]
        public async Task GetSinglePost_ReturnsRightPost_WhenValidGuidUsed()
        {
            // Arrange
            var guid = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619");

            //Act
            var controllerresult = await postsController.GetPost(guid);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var model = Assert.IsAssignableFrom<PostViewDto>(result.Value);
            Assert.Equal(guid, model.Id);
        }

        [Fact]
        public async Task GetPosts_ReturnsOkResult()
        {
            // Arrange

            //Act
            var result = await postsController.GetPosts(sieveModel.Object);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetPosts_Returns_IEnumerableOfPostViewDto()
        {
            // Arrange

            //Act
            var controllerresult = await postsController.GetPosts(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            Assert.IsAssignableFrom<IEnumerable<PostViewDto>>(result.Value);
        }

        [Fact]
        public async Task GetPosts_Returns_ExactlyTwo_PostViewDtos()
        {
            // Arrange

            //Act
            var controllerresult = await postsController.GetPosts(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var posts = Assert.IsAssignableFrom<IEnumerable<PostViewDto>>(result.Value);

            Assert.Equal(2, posts.Count());
        }

        [Fact]
        public async Task GetPosts_Returns_ExactlyOne_PostViewDto_WithTitleFilter()
        {
            // Arrange
            sieveModel.Object.Filters = "Title@=*1";
            mapper.Setup(map => map.Map<IEnumerable<PostViewDto>>(It.IsAny<IEnumerable<Post>>())).Returns(GetTestPostsViewDtoSingle);

            //Act
            var controllerresult = await postsController.GetPosts(sieveModel.Object);
            var result = controllerresult.Result as OkObjectResult;

            //Assert
            var posts = Assert.IsAssignableFrom<IEnumerable<PostViewDto>>(result.Value);

            Assert.Single(posts);
        }

        [Fact]
        public async Task Add_InvalidPost_Returns_BadRequest()
        {
            //Arrange
            PostInputDto x = null;

            //Act
            var result = await postsController.CreatePost(x);

            //Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task Add_InvalidPost_WithoutValidUser_Returns_BadRequest()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var result = await postsController.CreatePost(postInputDto.Object);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Add_InvalidPost_WithModelErrors_Returns_UnprocessableEntityRequest()
        {
            //Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            postsController.ModelState.AddModelError("X", "Invalid model state");

            //Act
            var result = await postsController.CreatePost(postInputDto.Object);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Add_ValidPost__Returns_CreatedAtRouteResult()
        {
            //Arrange

            //Act
            var result = await postsController.CreatePost(postInputDto.Object);

            //Assert
            Assert.IsType<CreatedAtRouteResult>(result.Result);
        }

        [Fact]
        public async Task Add_ValidPost__Returns_PostViewDto()
        {
            //Arrange

            //Act            
            var controllerresult = await postsController.CreatePost(postInputDto.Object);
            var result = controllerresult.Result as CreatedAtRouteResult;

            //Assert
            Assert.IsType<PostViewDto>(result.Value);
        }

        [Fact]
        public async Task Add_ValidPost__Returns_CreatedAtRouteResult_At_GetPost_Action()
        {
            //Arrange

            //Act
            var controllerresult = await postsController.CreatePost(postInputDto.Object);
            var result = controllerresult.Result as CreatedAtRouteResult;

            //Assert
            Assert.Equal("GetPost", result.RouteName);
        }

        [Fact]
        public async Task Checking_Existing_PostId__Returns_ObjectResult()
        {
            //Arrange

            //Act
            var result = await postsController.BlockPostCreation(Guid.NewGuid());

            //Assert
            Assert.IsType<ObjectResult>(result);
        }

        [Fact]
        public async Task Checking_Existing_PostId__Returns_409StatusCode()
        {
            //Arrange

            //Act
            var result = await postsController.BlockPostCreation(Guid.NewGuid()) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        }

        [Fact]
        public async Task Delete_Post_ThatDoesNotExist_Returns_NotFound()
        {
            //Arrange
            postRepository.Setup(repo => repo.GetPostAsync(It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var result = await postsController.DeletePost(Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Post_OfDifferentUser_Returns_ForbiddenStatusCode()
        {
            //Arrange
            postRepository.Setup(repo => repo.GetPostAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestPost);
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var result = await postsController.DeletePost(Guid.NewGuid()) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Delete_Post_Returns_NoContent()
        {
            //Arrange
            postRepository.Setup(repo => repo.GetPostAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestPost);

            //Act
            var result = await postsController.DeletePost(Guid.NewGuid());

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_WithInValidPost_Returns_UnProcessableEntityResult()
        {
            //Arrange
            postsController.ModelState.AddModelError("", "Error");

            //Act
            var result = await postsController.UpdatePost(Guid.NewGuid(), postUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_Post_Returns_NoContent()
        {
            //Arrange
            postRepository.Setup(repo => repo.GetPostAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestPost);

            //Act
            var result = await postsController.UpdatePost(Guid.NewGuid(), postUpdateDto.Object);

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_PostNotInDatabase_Returns_NotFoundResult()
        {
            //Arrange

            //Act
            var result = await postsController.UpdatePost(Guid.NewGuid(), postUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_OnPut_With_DatabaseSaveError_Throws_Exception()
        {
            //Arrange
            postRepository.Setup(repo => repo.GetPostAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestPost);
            postRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => false);

            //Act

            //Assert
            await Assert.ThrowsAsync<Exception>(() => postsController.UpdatePost(Guid.NewGuid(), postUpdateDto.Object));
        }

        [Fact]
        public async Task Update_OnPatch_With_DatabaseSaveError_Throws_Exception()
        {
            //Arrange
            postRepository.Setup(repo => repo.GetPostAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestPost);
            postRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => false);
            jsonPatchPostUpdateDto.Object.Replace(x => x.Title, "ABC");
            JsonPatchDocument<PostUpdateDto> test = new JsonPatchDocument<PostUpdateDto>();
            test.Replace(x => x.Title, "ABC");

            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(),
                                              It.IsAny<ValidationStateDictionary>(),
                                              It.IsAny<string>(),
                                              It.IsAny<Object>()))
                                              .Throws(new Exception());
            postsController.ObjectValidator = objectValidator.Object;

            //Act

            //Assert
            await Assert.ThrowsAsync<Exception>(() => postsController.PartialUpdatePost(Guid.NewGuid(), test));
        }

        private PostUpdateDto GetTestPostUpdateDto()
        {
            return new PostUpdateDto
            {
                Title = "test title",
                Message = "Test message",
                Important = false
            };
        }

        private PostInputDto GetPostForInput()
        {
            return new PostInputDto
            {
                Title = "test title",
                Message = "Test message",
                Important = false
            };
        }

        private PostViewDto GetTestPostViewDto()
        {
            var posts = new PostViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Message = "Test1",
                Important = false,
                DaysOld = 0,
                Created = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com"
            };

            return posts;
        }

        private IEnumerable<PostViewDto> GetTestPostsViewDto()
        {
            var posts = new List<PostViewDto>();
            posts.Add(new PostViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Message = "Test1",
                Important = false,
                DaysOld = 0,
                Created = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com"
            });
            posts.Add(new PostViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                Title = "Test2",
                Message = "Test2",
                Important = false,
                DaysOld = 0,
                Created = DateTime.Now,
                OwnerName = "XYZ",
                OwnerEmail = "Test@test.com"
            });

            return posts;
        }

        private IEnumerable<PostViewDto> GetTestPostsViewDtoSingle()
        {
            var posts = new List<PostViewDto>();
            posts.Add(new PostViewDto()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Message = "Test1",
                Important = false,
                DaysOld = 0,
                Created = DateTime.Now,
                OwnerName = "ABC",
                OwnerEmail = "Test@test.com"
            });
            return posts;
        }

        private Post GetTestPost()
        {
            var posts = new Post()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Message = "Test1",
                Important = false,
                Created = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            };

            return posts;
        }

        private IEnumerable<Post> GetTestPosts()
        {
            var posts = new List<Post>();
            posts.Add(new Post()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Message = "Test1",
                Important = false,
                Created = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            });
            posts.Add(new Post()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1618"),
                Title = "Test2",
                Message = "Test2",
                Important = false,
                Created = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            });
            return posts.AsEnumerable();
        }

        private IEnumerable<Post> GetTestPostsSinglePost()
        {
            var posts = new List<Post>();
            posts.Add(new Post()
            {
                Id = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Title = "Test1",
                Message = "Test1",
                Important = false,
                Created = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            });
            return posts.AsEnumerable();
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
