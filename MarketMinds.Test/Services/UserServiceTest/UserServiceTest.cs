using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.UserService;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserProxyRepository> mockRepository;
        private readonly UserService userService;
        private readonly JsonSerializerOptions jsonOptions;

        public UserServiceTests()
        {
            mockRepository = new Mock<IUserProxyRepository>();
            jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            userService = new UserService(mockRepository.Object, jsonOptions);
        }

        [Fact]
        public async Task GetUserByIdAsync_ThrowsException_WhenIdIsInvalid()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => userService.GetUserByIdAsync(0));
            Assert.Equal("User ID must be a positive number. (Parameter 'userId')", exception.Message);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenFound()
        {
            var expectedUser = new User { Id = 1, Username = "test", Email = "a@b.com" };
            string json = JsonSerializer.Serialize(expectedUser, jsonOptions);
            mockRepository.Setup(r => r.GetUserByIdRawAsync(1)).ReturnsAsync(json);

            var result = await userService.GetUserByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("test", result.Username);
        }

        [Fact]
        public async Task GetUserOrdersAsync_ReturnsOrders()
        {
            var expectedOrders = new List<UserOrder> { new UserOrder { OrderId = 42 } };
            string json = JsonSerializer.Serialize(expectedOrders, jsonOptions);
            mockRepository.Setup(r => r.GetUserOrdersRawAsync(1)).ReturnsAsync(json);

            var result = await userService.GetUserOrdersAsync(1);

            Assert.Single(result);
            Assert.Equal(42, result[0].OrderId);
        }

        [Fact]
        public async Task GetBasketTotalAsync_ReturnsValue()
        {
            double expected = 99.99;
            string json = JsonSerializer.Serialize(expected, jsonOptions);
            mockRepository.Setup(r => r.GetBasketTotalRawAsync(1, 2)).ReturnsAsync(json);

            double total = await userService.GetBasketTotalAsync(1, 2);

            Assert.Equal(expected, total);
        }

        [Fact]
        public async Task CreateOrderFromBasketAsync_ReturnsOrders()
        {
            var orders = new List<Order> { new Order { Id = 1 } };
            string json = JsonSerializer.Serialize(orders, jsonOptions);
            mockRepository.Setup(r => r.CreateOrderFromBasketRawAsync(1, 2, 0)).ReturnsAsync(json);

            var result = await userService.CreateOrderFromBasketAsync(1, 2);

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsTrue_OnSuccess()
        {
            var user = new User { Id = 1, Username = "test", Email = "test@test.com" };
            mockRepository.Setup(r => r.UpdateUserRawAsync(It.IsAny<MarketMinds.Shared.Models.User>())).ReturnsAsync(true);

            var result = await userService.UpdateUserAsync(user);

            Assert.True(result);
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsException_WhenUserIsNull()
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => userService.UpdateUserAsync(null));
            Assert.Equal("User cannot be null. (Parameter 'user')", exception.Message);
        }
    }
}
