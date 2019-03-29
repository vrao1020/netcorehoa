using HoaInfrastructure.Context;
using HoaEntities.Entities;
using HoaInfrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Sieve.Services;
using System;
using HoaCommon.Services;

namespace HoaWebAPIUnitTests.ControllerTests
{
    public class EventRepositoryShould
    {
        private EventRepository eventRepository;
        private Mock<HoaDbContext> _context;
        private Mock<ISieveProcessor> _sieveProcessor;
        private Mock<IPaginationGenerator> _paginationGenerator;
        private Mock<IConfiguration> _configuration;
        private Mock<DbSet<Event>> _dbSetMock;

        public EventRepositoryShould()
        {
            _context = new Mock<HoaDbContext>();
            _sieveProcessor = new Mock<ISieveProcessor>();
            _paginationGenerator = new Mock<IPaginationGenerator>();
            _configuration = new Mock<IConfiguration>();
            _dbSetMock = new Mock<DbSet<Event>>();
            eventRepository = new EventRepository(_context.Object);
        }

        //[Fact]
        //public async Task GetEventAsync_WithValidId_ShouldReturn_SingleEvent_()
        //{
        //    //Arrange
        //    _context.Setup(x => x.Set<Event>()).Returns(_dbSetMock.Object);

        //    _dbSetMock.Setup(x => x.FindAsync(It.IsAny<Guid>())).ReturnsAsync(GetTestEvent);

        //    //Act
        //    var result = await eventRepository.GetEventAsync(Guid.NewGuid());

        //    //Assert
        //    Assert.Equal("Test1", result.Title);
        //}

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
    }
}
