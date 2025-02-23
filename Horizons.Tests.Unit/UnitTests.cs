using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Horizons.Controllers;
using Horizons.Data;
using Horizons.Data.Models;
using Horizons.Models;
using Horizons.Services;

namespace Horizons.UnitTests
{
    [TestFixture]
    public class DestinationControllerTests
    {
        private ApplicationDbContext _dbContext;
        private DestinationController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "HorizonsDbTest")
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _controller = new DestinationController(new DestinationService(_dbContext));

            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testUserId"),
            }));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = mockUser }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task Add_Get_ShouldReturnViewWithTerrains()
        {
            // Act
            var result = await _controller.Add();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as DestinationAddViewModel;
            Assert.IsNotNull(model);
            Assert.NotNull(model.Terrains);
        }

        [Test]
        public async Task Add_Post_ShouldRedirectToIndex_OnValidModel()
        {
            // Arrange
            var destinationFormModel = new DestinationAddViewModel()
            {
                Name = "Test Destination",
                Description = "Test Description",
                TerrainId = 1,
                PublishedOn = DateTime.UtcNow.ToString("yyyy-MM-dd")
            };

            // Act
            var result = await _controller.Add(destinationFormModel);

            // Assert
            var redirectToActionResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectToActionResult);
            Assert.AreEqual("Index", redirectToActionResult.ActionName);
        }

        [Test]
        public async Task Edit_Get_ShouldReturnViewWithCorrectModel_WhenDestinationExists()
        {
            // Arrange
            var destination = new Destination()
            {
                Id = 1,
                Name = "Test Destination",
                Description = "Test Description",
                TerrainId = 1,
                PublisherId = "testUserId",
                PublishedOn = DateTime.UtcNow
            };
            _dbContext.Destinations.Add(destination);
            _dbContext.SaveChanges();

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as DestinationEditViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual("Test Destination", model.Name);
        }

        [Test]
        public async Task AddToFavorites_ShouldRedirectToIndex_WhenDestinationIsAdded()
        {
            // Arrange
            var destination = new Destination()
            {
                Id = 1,
                Name = "Test Destination",
                Description = "Description",
                TerrainId = 1,
                PublisherId = "testUserId",
                PublishedOn = DateTime.UtcNow
            };
            _dbContext.Destinations.Add(destination);
            _dbContext.SaveChanges();

            // Act
            var result = await _controller.AddToFavorites(1);

            // Assert
            var redirectToActionResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectToActionResult);
            Assert.AreEqual("Index", redirectToActionResult.ActionName);
        }
    }
}
