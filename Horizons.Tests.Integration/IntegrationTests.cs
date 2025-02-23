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

namespace Horizons.IntegrationTests
{
    [TestFixture]
    public class DestinationControllerIntegrationTests
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
        public async Task AddDestination_ShouldInsertDestinationIntoDatabase()
        {
            var model = new DestinationAddViewModel()
            {
                Name = "Integration Test Destination",
                Description = "Test Description",
                TerrainId = 1,
                PublishedOn = DateTime.UtcNow.ToString("yyyy-MM-dd") // Format as string
            };

            var result = await _controller.Add(model);

            var redirectToActionResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectToActionResult);
            Assert.AreEqual("Index", redirectToActionResult.ActionName);

            var destinationsInDb = _dbContext.Destinations.ToList();
            Assert.AreEqual(1, destinationsInDb.Count);
            Assert.AreEqual("Integration Test Destination", destinationsInDb[0].Name);
        }

        [Test]
        public async Task EditDestination_ShouldUpdateDestinationInDatabase()
        {
            var destination = new Destination
            {
                Id = 1,
                Name = "Original Destination",
                Description = "Original Description",
                TerrainId = 1,
                PublisherId = "testUserId",
                PublishedOn = DateTime.UtcNow
            };
            _dbContext.Destinations.Add(destination);
            await _dbContext.SaveChangesAsync();

            var editModel = new DestinationEditViewModel()
            {
                Id = 1,
                Name = "Edited Destination",
                Description = "Edited Description",
                TerrainId = 1,
                PublisherId = "testUserId",
                PublishedOn = DateTime.UtcNow.ToString("yyyy-MM-dd") // Format as string
            };

            var result = await _controller.Edit(editModel);

            var redirectToActionResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectToActionResult);
            Assert.AreEqual("Details", redirectToActionResult.ActionName);

            var updatedDestination = await _dbContext.Destinations.FindAsync(destination.Id);
            Assert.AreEqual("Edited Destination", updatedDestination.Name);
            Assert.AreEqual("Edited Description", updatedDestination.Description);
        }
    }
}
