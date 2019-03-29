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
    public class CommentControllerShould
    {
        private CommentsController commentsController;
        private Mock<ICommentRepository> commentRepository;
        private Mock<IPostRepository> postRepository;
        private Mock<IUserRepository> userRepository;
        private Mock<IMapper> mapper;
        private Mock<SieveModel> sieveModel;
        private Mock<CommentInputDto> commentInputDto;
        private Mock<CommentUpdateDto> commentUpdateDto;
        private Mock<JsonPatchDocument<CommentUpdateDto>> jsonPatchPostUpdateDto;
        private Mock<ISortFilterService<Comment>> sortFilterService;

        public CommentControllerShould()
        {
            commentRepository = new Mock<ICommentRepository>();
            userRepository = new Mock<IUserRepository>();
            mapper = new Mock<IMapper>();
            sieveModel = new Mock<SieveModel>();
            commentInputDto = new Mock<CommentInputDto>();
            commentUpdateDto = new Mock<CommentUpdateDto>();
            jsonPatchPostUpdateDto = new Mock<JsonPatchDocument<CommentUpdateDto>>();
            postRepository = new Mock<IPostRepository>();
            sortFilterService = new Mock<ISortFilterService<Comment>>();

            //// Arrange            
            commentRepository.Setup(repo => repo.GetCommentAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(GetTestComment);
            commentRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(() => true);
            commentRepository.Setup(repo => repo.GetComments(It.IsAny<Guid>())).Returns(() => null);

            postRepository.Setup(repo => repo.PostExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            mapper.Setup(map => map.Map<IEnumerable<CommentViewDto>>(It.IsAny<IEnumerable<Comment>>())).Returns(GetTestCommentViewDtos);
            mapper.Setup(map => map.Map<CommentViewDto>(It.IsAny<Comment>())).Returns(GetTestCommentViewDto);

            mapper.Setup(map => map.Map<Comment>(It.IsAny<CommentInputDto>())).Returns(GetTestComment);
            mapper.Setup(map => map.Map<CommentUpdateDto>(It.IsAny<Comment>())).Returns(GetTestCommentUpdateDto);

            commentsController = new CommentsController(commentRepository.Object, mapper.Object, userRepository.Object, postRepository.Object, sortFilterService.Object);

            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUser);
        }

        [Fact]
        public async Task GetComments_WithInvalidPost_Returns_NotFound()
        {
            // Arrange
            postRepository.Setup(repo => repo.PostExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            //Act
            var response = await commentsController.GetCommentsForPost(Guid.NewGuid(), sieveModel.Object);

            //Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetComments_WithValidPost_Returns_OkObjectResult()
        {
            // Arrange

            //Act
            var response = await commentsController.GetCommentsForPost(Guid.NewGuid(), sieveModel.Object);

            //Assert
            Assert.IsType<OkObjectResult>(response.Result);
        }

        [Fact]
        public async Task GetComments_WithValidPost_Returns_IEnumerable_CommentViewDto()
        {
            // Arrange

            //Act
            var result = await commentsController.GetCommentsForPost(Guid.NewGuid(), sieveModel.Object);
            var response = result.Result as OkObjectResult;

            //Assert
            Assert.IsAssignableFrom<IEnumerable<CommentViewDto>>(response.Value);
        }

        [Fact]
        public async Task GetComments_WithValidPost_Returns_Two_CommentViewDto()
        {
            // Arrange

            //Act
            var result = await commentsController.GetCommentsForPost(Guid.NewGuid(), sieveModel.Object);
            var response = result.Result as OkObjectResult;
            var collection = response.Value as IEnumerable<CommentViewDto>;

            //Assert
            Assert.Equal(2, collection.Count());
        }

        [Fact]
        public async Task GetComment_WithInvalidPost_Returns_NotFound()
        {
            // Arrange
            postRepository.Setup(repo => repo.PostExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            //Act
            var response = await commentsController.GetCommentForPost(Guid.NewGuid(), Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetComment_WithValidPost_Returns_OkObjectResult()
        {
            // Arrange

            //Act
            var response = await commentsController.GetCommentForPost(Guid.NewGuid(), Guid.NewGuid());

            //Assert
            Assert.IsType<OkObjectResult>(response.Result);
        }

        [Fact]
        public async Task GetComment_WithValidPost_Returns_CommentViewDto()
        {
            // Arrange

            //Act
            var result = await commentsController.GetCommentForPost(Guid.NewGuid(), Guid.NewGuid());
            var response = result.Result as OkObjectResult;

            //Assert
            Assert.IsAssignableFrom<CommentViewDto>(response.Value);
        }

        [Fact]
        public async Task GetComment_NotInRepo_Returns_NotFound()
        {
            // Arrange
            commentRepository.Setup(repo => repo.GetCommentAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var response = await commentsController.GetCommentForPost(Guid.NewGuid(), Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task Create_InvalidComment_Returns_BadRequest()
        {
            // Arrange

            //Act
            var response = await commentsController.CreateCommentForPost(Guid.NewGuid(), null);

            //Assert
            Assert.IsType<BadRequestResult>(response.Result);
        }

        [Fact]
        public async Task Create_Comment_ForPostNotExists_Returns_BadRequest()
        {
            // Arrange
            postRepository.Setup(repo => repo.PostExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            //Act
            var response = await commentsController.CreateCommentForPost(Guid.NewGuid(), null);

            //Assert
            Assert.IsType<BadRequestResult>(response.Result);
        }

        [Fact]
        public async Task Create_Comment_ForUserNotExists_Returns_BadRequest()
        {
            // Arrange
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var response = await commentsController.CreateCommentForPost(Guid.NewGuid(), null);

            //Assert
            Assert.IsType<BadRequestResult>(response.Result);
        }

        [Fact]
        public async Task Create_Comment_InvalidModel_Returns_BadRequest()
        {
            // Arrange
            commentsController.ModelState.AddModelError("x", "x");
            userRepository.Setup(user => user.GetUserAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var response = await commentsController.CreateCommentForPost(Guid.NewGuid(), commentInputDto.Object);

            //Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task Create_Comment_Returns_CreatedAtRouteResult()
        {
            // Arrange            

            //Act
            var response = await commentsController.CreateCommentForPost(Guid.NewGuid(), commentInputDto.Object);

            //Assert
            Assert.IsType<CreatedAtRouteResult>(response.Result);
        }

        [Fact]
        public async Task Create_Comment_WithSaveError_Returns_Exception()
        {
            // Arrange            
            commentRepository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(() => false);

            //Act

            //Assert
            await Assert.ThrowsAsync<Exception>(() => commentsController.CreateCommentForPost(Guid.NewGuid(), commentInputDto.Object));
        }

        [Fact]
        public async Task Create_Comment_Returns_CommentViewDto()
        {
            // Arrange            

            //Act            
            var result = await commentsController.CreateCommentForPost(Guid.NewGuid(), commentInputDto.Object);
            var response = result.Result as CreatedAtRouteResult;

            //Assert
            Assert.IsType<CommentViewDto>(response.Value);
        }

        [Fact]
        public async Task Create_Comment_Returns_CreatedAt_GetComment_Result()
        {
            // Arrange            

            //Act
            var result = await commentsController.CreateCommentForPost(Guid.NewGuid(), commentInputDto.Object);
            var response = result.Result as CreatedAtRouteResult;

            //Assert
            Assert.Equal("GetComment", response.RouteName);
        }

        [Fact]
        public async Task Update_WithPut_InvalidComment_Returns_NotFoundResult()
        {
            // Arrange            
            commentRepository.Setup(repo => repo.GetCommentAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var response = await commentsController.UpdateCommentForPost(Guid.NewGuid(), Guid.NewGuid(), null);

            //Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task Update_WithPut_Comment_ForPostNotExists_Returns_NotFoundResult()
        {
            // Arrange
            postRepository.Setup(repo => repo.PostExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            //Act
            var response = await commentsController.UpdateCommentForPost(Guid.NewGuid(), Guid.NewGuid(), commentUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task Update_WithPut_Comment_NotOwnerOrAdmin_Returns_403Forbidden()
        {
            // Arrange
            userRepository.Setup(x => x.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUserOther);

            //Act
            var response = await commentsController.UpdateCommentForPost(Guid.NewGuid(), Guid.NewGuid(), commentUpdateDto.Object) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status403Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithPut_Comment_AsOwner_Returns_NoContent()
        {
            // Arrange

            //Act
            var response = await commentsController.UpdateCommentForPost(Guid.NewGuid(), Guid.NewGuid(), commentUpdateDto.Object);

            //Assert
            Assert.IsType<NoContentResult>(response);
        }

        [Fact]
        public async Task Update_WithPatch_InvalidPatchDoc_Returns_BadRequest()
        {
            // Arrange            

            //Act
            var response = await commentsController.PartialUpdateCommentForPost(Guid.NewGuid(), Guid.NewGuid(), null);

            //Assert
            Assert.IsType<BadRequestResult>(response);
        }

        [Fact]
        public async Task Update_WithPatch_InvalidPost_Returns_NotFound()
        {
            // Arrange
            postRepository.Setup(repo => repo.PostExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            //Act
            var response = await commentsController.PartialUpdateCommentForPost(Guid.NewGuid(), Guid.NewGuid(), jsonPatchPostUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task Update_WithPatch_InvalidComment_Returns_NotFound()
        {
            // Arrange
            commentRepository.Setup(x => x.GetCommentAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var response = await commentsController.PartialUpdateCommentForPost(Guid.NewGuid(), Guid.NewGuid(), jsonPatchPostUpdateDto.Object);

            //Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task Update_WithPatch_Comment_NotOwnerOrAdmin_Returns_403Forbidden()
        {
            // Arrange
            userRepository.Setup(x => x.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUserOther);

            //Act
            var response = await commentsController.PartialUpdateCommentForPost(Guid.NewGuid(), Guid.NewGuid(), jsonPatchPostUpdateDto.Object) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status403Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithPatch_Comment_AsOwner_WithModelStateError_Returns_Unprocessable()
        {
            // Arrange
            jsonPatchPostUpdateDto.Object.Replace(x => x.Message, "abcdefg");

            //need to mock this as TryValidateModel will throw an error unless objectmodelvalidator is setup
            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(),
                                              It.IsAny<ValidationStateDictionary>(),
                                              It.IsAny<string>(),
                                              It.IsAny<Object>()));
            commentsController.ObjectValidator = objectValidator.Object;

            commentsController.ModelState.AddModelError("x", "x");

            //Act
            var response = await commentsController.PartialUpdateCommentForPost(Guid.NewGuid(), Guid.NewGuid(), jsonPatchPostUpdateDto.Object);

            //Assert
            Assert.IsType<UnprocessableEntityObjectResult>(response);
        }

        [Fact]
        public async Task Update_WithPatch_Comment_AsOwner_Returns_NoContent()
        {
            // Arrange
            jsonPatchPostUpdateDto.Object.Replace(x => x.Message, "abcdefg");
            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(o => o.Validate(It.IsAny<ActionContext>(),
                                              It.IsAny<ValidationStateDictionary>(),
                                              It.IsAny<string>(),
                                              It.IsAny<Object>()));
            commentsController.ObjectValidator = objectValidator.Object;

            //Act
            var response = await commentsController.PartialUpdateCommentForPost(Guid.NewGuid(), Guid.NewGuid(), jsonPatchPostUpdateDto.Object);

            //Assert
            Assert.IsType<NoContentResult>(response);
        }

        [Fact]
        public async Task DeleteComment_InvalidPost_Returns_NotFound()
        {
            // Arrange
            postRepository.Setup(repo => repo.PostExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            //Act
            var response = await commentsController.DeleteCommentForPost(Guid.NewGuid(), Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task Delete_InvalidComment_Returns_NotFound()
        {
            // Arrange
            commentRepository.Setup(x => x.GetCommentAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(() => null);

            //Act
            var response = await commentsController.DeleteCommentForPost(Guid.NewGuid(), Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task Delete_Comment_NotOwnerOrAdmin_Returns_403Forbidden()
        {
            // Arrange
            userRepository.Setup(x => x.GetUserAsync(It.IsAny<string>())).ReturnsAsync(GetTestUserOther);

            //Act
            var response = await commentsController.DeleteCommentForPost(Guid.NewGuid(), Guid.NewGuid()) as ObjectResult;

            //Assert
            Assert.Equal(StatusCodes.Status403Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Delete_Comment_AsOwner_Returns_NoContent()
        {
            // Arrange

            //Act
            var response = await commentsController.DeleteCommentForPost(Guid.NewGuid(), Guid.NewGuid());

            //Assert
            Assert.IsType<NoContentResult>(response);
        }

        private IEnumerable<Comment> GetTestComments()
        {
            return new List<Comment>()
            {
                new Comment
                {
                    Id = Guid.NewGuid(),
                    Message = "Test1",
                    ParentCommentId = Guid.NewGuid(),
                    PostId = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                    Created = DateTime.Now,
                    UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    Message = "Test2",
                    ParentCommentId = Guid.NewGuid(),
                    PostId = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                    Created = DateTime.Now,
                    UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c8")
                }
            };
        }

        private IEnumerable<CommentViewDto> GetTestCommentViewDtos()
        {
            return new List<CommentViewDto>()
            {
                new CommentViewDto
                {
                    Id = Guid.NewGuid(),
                    Message = "Test1",
                    ParentCommentId = Guid.NewGuid(),
                    PostId = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                    Created = DateTime.Now,
                    DaysOld = 1,
                    OwnerEmail = "abc",
                    OwnerName = "abc"
                },
                new CommentViewDto
                {
                    Id = Guid.NewGuid(),
                    Message = "Test2",
                    ParentCommentId = Guid.NewGuid(),
                    PostId = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                    Created = DateTime.Now,
                    DaysOld = 1,
                    OwnerEmail = "abc",
                    OwnerName = "abc"
                }
            };
        }

        private Comment GetTestComment()
        {
            return new Comment
            {
                Id = Guid.NewGuid(),
                Message = "Test1",
                ParentCommentId = Guid.NewGuid(),
                PostId = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Created = DateTime.Now,
                UserId = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c9")
            };

        }

        private CommentViewDto GetTestCommentViewDto()
        {
            return new CommentViewDto
            {
                Id = Guid.NewGuid(),
                Message = "Test1",
                ParentCommentId = Guid.NewGuid(),
                PostId = new Guid("e77551ba-78e2-4a36-8754-3ea5f12e1619"),
                Created = DateTime.Now,
                DaysOld = 1,
                OwnerEmail = "abc",
                OwnerName = "abc"
            };
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

        private User GetTestUserOther()
        {
            return new User
            {
                Id = new Guid("5f76bd52-b065-487a-89ca-c9ec6a9b60c8"),
                SocialId = "zqSdVijp7rK9hsg5BhKNvfOzNFxSBRcl@clients",
                FirstName = "A",
                LastName = "B",
                Reminder = false,
                Email = "abc@xyz.com",
                Joined = DateTime.Now
            };
        }

        private CommentUpdateDto GetTestCommentUpdateDto()
        {
            return new CommentUpdateDto
            {
                Message = "Abc"
            };
        }

        private CommentInputDto GetCommentForInput()
        {
            return new CommentInputDto
            {
                Message = "abc",
                ParentCommentId = Guid.NewGuid()
            };
        }

    }
}
