/*
using System;
using System.Collections.Generic;
using NUnit.Framework;
using DomainLayer.Domain;
using Marketplace_SE.Data;
using Marketplace_SE.Services.DreamTeam;
using Moq;

namespace Marketplace_SE.Tests.Services.DreamTeam
{
    [TestFixture]
    public class AccountPageServiceTest
    {
        // Constants for test data
        private const int VALID_USER_ID = 1;
        private const int INVALID_USER_ID = -1;
        private const string TEST_USERNAME = "test";

        // Mock database data
        private List<Dictionary<string, object>> _mockOrderData;
        private List<UserOrder> _mockOrders;

        private AccountPageService _accountPageService;
        private Mock<IDatabase> _mockDatabase;

        [SetUp]
        public void Setup()
        {
            // Setup mock database
            _mockDatabase = new Mock<IDatabase>();

            // Setup mock order data
            _mockOrderData = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "Id", 1 },
                    { "Created", DateTime.Now.AddDays(-1).Ticks },
                    { "sellerId", VALID_USER_ID },
                    { "buyerId", 2 }
                },
                new Dictionary<string, object>
                {
                    { "Id", 2 },
                    { "Created", DateTime.Now.Ticks },
                    { "sellerId", 3 },
                    { "buyerId", VALID_USER_ID }
                }
            };

            _mockOrders = new List<UserOrder>
            {
                new UserOrder { Id = 2, Created = DateTime.Now.Ticks, SellerId = 3, BuyerId = VALID_USER_ID },
                new UserOrder { Id = 1, Created = DateTime.Now.AddDays(-1).Ticks, SellerId = VALID_USER_ID, BuyerId = 2 }
            };

            // Configure mock database behavior
            _mockDatabase.Setup(db => db.Connect()).Returns(true);
            _mockDatabase.Setup(db => db.Get(
                It.IsAny<string>(),
                It.IsAny<string[]>(),
                It.IsAny<object[]>()))
                .Returns(_mockOrderData);
            _mockDatabase.Setup(db => db.ConvertToObject<UserOrder>(_mockOrderData))
                .Returns(_mockOrders);

            // Create service with mock database
            Database.Databases = _mockDatabase.Object;
            _accountPageService = new AccountPageService();
        }

        [Test]
        public void TestGetCurrentUser_ReturnsTestUser()
        {
            // Act
            var result = _accountPageService.GetCurrentUser();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo(TEST_USERNAME));
        }

        [Test]
        public void TestGetUserOrders_ValidUserId_ReturnsOrders()
        {
            // Act
            var result = _accountPageService.GetUserOrders(VALID_USER_ID);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestGetUserOrders_OrdersAreSortedByCreatedDescending()
        {
            // Act
            var result = _accountPageService.GetUserOrders(VALID_USER_ID);

            // Assert
            Assert.That(result[0].Id, Is.EqualTo(2)); // Newest first
            Assert.That(result[1].Id, Is.EqualTo(1));
        }

        [Test]
        public void TestGetUserOrders_InvalidUserId_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<Exception>(() => _accountPageService.GetUserOrders(INVALID_USER_ID));
        }

        [Test]
        public void TestGetUserOrders_DatabaseConnectionFails_ThrowsException()
        {
            // Arrange
            _mockDatabase.Setup(db => db.Connect()).Returns(false);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _accountPageService.GetUserOrders(VALID_USER_ID));
            Assert.That(ex.Message, Is.EqualTo("Database connection error"));
        }

        [Test]
        public void TestGetUserOrders_NoOrdersFound_ReturnsEmptyList()
        {
            // Arrange
            _mockDatabase.Setup(db => db.Get(
                It.IsAny<string>(),
                It.IsAny<string[]>(),
                It.IsAny<object[]>()))
                .Returns(new List<Dictionary<string, object>>());

            // Act
            var result = _accountPageService.GetUserOrders(VALID_USER_ID);

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}

*/