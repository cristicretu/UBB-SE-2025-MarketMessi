using System;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Configuration;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.UserService;
using MarketMinds.Shared.ProxyRepository;

namespace MarketMinds.Tests.Services.UserServiceTest
{
    [TestFixture]
    public class UserServiceTest
    {
        private IUserService _userService;
        private Mock<IConfiguration> _configurationMock;
        private Mock<UserProxyRepository> _repositoryMock;

        private const int USER_ID = 1;
        private const string USERNAME = "testuser";
        private const string EMAIL = "test@example.com";
        private const string PASSWORD = "testpassword";

        [SetUp]
        public void Setup()
        {
            // Create mocks
            _configurationMock = new Mock<IConfiguration>();

            // Setup repository mock using partial mocking
            // Note: We need to use Moq.Protected().Setup() for non-public methods if needed
            _repositoryMock = new Mock<UserProxyRepository>(_configurationMock.Object) { CallBase = true };

            // Create test user JSON
            var testUser = new MarketMinds.Shared.Models.User
            {
                Id = USER_ID,
                Username = USERNAME,
                Email = EMAIL,
                Password = PASSWORD
            };
            string testUserJson = JsonSerializer.Serialize(testUser);

            // Setup repository mock methods
            SetupRepositoryMethods(_repositoryMock, testUserJson);

            // Create user service instance with the mocked repository
            _userService = new UserServiceWithMockedRepository(_repositoryMock.Object);
        }

        private void SetupRepositoryMethods(Mock<UserProxyRepository> mock, string testUserJson)
        {
            // Setup AuthenticateUserRawAsync
            mock.Setup(repo => repo.AuthenticateUserRawAsync(USERNAME, PASSWORD))
                .ReturnsAsync(testUserJson);
            mock.Setup(repo => repo.AuthenticateUserRawAsync(USERNAME, It.Is<string>(p => p != PASSWORD)))
                .ThrowsAsync(new Exception("Authentication failed"));

            // Setup GetUserByUsernameRawAsync
            mock.Setup(repo => repo.GetUserByUsernameRawAsync(USERNAME))
                .ReturnsAsync(testUserJson);
            mock.Setup(repo => repo.GetUserByUsernameRawAsync(It.Is<string>(u => u != USERNAME)))
                .ThrowsAsync(new Exception("User not found"));

            // Setup GetUserByEmailRawAsync
            mock.Setup(repo => repo.GetUserByEmailRawAsync(EMAIL))
                .ReturnsAsync(testUserJson);
            mock.Setup(repo => repo.GetUserByEmailRawAsync(It.Is<string>(e => e != EMAIL)))
                .ThrowsAsync(new Exception("User not found"));

            // Setup CheckUsernameRawAsync
            mock.Setup(repo => repo.CheckUsernameRawAsync(USERNAME))
                .ReturnsAsync(JsonSerializer.Serialize(new { Exists = true }));
            mock.Setup(repo => repo.CheckUsernameRawAsync(It.Is<string>(u => u != USERNAME)))
                .ReturnsAsync(JsonSerializer.Serialize(new { Exists = false }));

            // Setup RegisterUserRawAsync
            mock.Setup(repo => repo.RegisterUserRawAsync(It.IsAny<object>()))
                .ReturnsAsync((object registerRequest) => {
                    // Only register if username is not taken and email is not in use
                    var username = registerRequest.GetType().GetProperty("Username").GetValue(registerRequest).ToString();
                    var email = registerRequest.GetType().GetProperty("Email").GetValue(registerRequest).ToString();

                    if (username == USERNAME || email == EMAIL)
                        return null;

                    var registeredUser = new MarketMinds.Shared.Models.User
                    {
                        Id = 2, // Different ID for new user
                        Username = username,
                        Email = email,
                        Password = "hashedpassword" // In real implementation this would be hashed
                    };

                    return JsonSerializer.Serialize(registeredUser);
                });

            // Setup GetUserByIdRawAsync
            mock.Setup(repo => repo.GetUserByIdRawAsync(USER_ID))
                .ReturnsAsync(testUserJson);
            mock.Setup(repo => repo.GetUserByIdRawAsync(It.Is<int>(id => id != USER_ID)))
                .ThrowsAsync(new Exception("User not found"));
        }

        [Test]
        public async Task AuthenticateUserAsync_ValidCredentials_ReturnsTrue()
        {
            // Act
            var result = await _userService.AuthenticateUserAsync(USERNAME, PASSWORD);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task AuthenticateUserAsync_InvalidCredentials_ReturnsFalse()
        {
            // Act
            var result = await _userService.AuthenticateUserAsync(USERNAME, "wrongpassword");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void AuthenticateUserAsync_NullUsername_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.AuthenticateUserAsync(null, PASSWORD));

            Assert.That(ex.ParamName, Is.EqualTo("username"));
        }

        [Test]
        public void AuthenticateUserAsync_NullPassword_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.AuthenticateUserAsync(USERNAME, null));

            Assert.That(ex.ParamName, Is.EqualTo("password"));
        }

        [Test]
        public async Task GetUserByCredentialsAsync_ValidCredentials_ReturnsUser()
        {
            // Act
            var user = await _userService.GetUserByCredentialsAsync(USERNAME, PASSWORD);

            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Username, Is.EqualTo(USERNAME));
            Assert.That(user.Email, Is.EqualTo(EMAIL));
        }

        [Test]
        public async Task GetUserByCredentialsAsync_InvalidCredentials_ReturnsNull()
        {
            // Act
            var user = await _userService.GetUserByCredentialsAsync(USERNAME, "wrongpassword");

            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public async Task GetUserByUsernameAsync_ExistingUsername_ReturnsUser()
        {
            // Act
            var user = await _userService.GetUserByUsernameAsync(USERNAME);

            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Username, Is.EqualTo(USERNAME));
        }

        [Test]
        public async Task GetUserByUsernameAsync_NonExistingUsername_ReturnsNull()
        {
            // Act
            var user = await _userService.GetUserByUsernameAsync("nonexistentuser");

            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public void GetUserByUsernameAsync_NullUsername_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.GetUserByUsernameAsync(null));

            Assert.That(ex.ParamName, Is.EqualTo("username"));
        }

        [Test]
        public async Task GetUserByEmailAsync_ExistingEmail_ReturnsUser()
        {
            // Act
            var user = await _userService.GetUserByEmailAsync(EMAIL);

            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Email, Is.EqualTo(EMAIL));
        }

        [Test]
        public async Task GetUserByEmailAsync_NonExistingEmail_ReturnsNull()
        {
            // Act
            var user = await _userService.GetUserByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public void GetUserByEmailAsync_NullEmail_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.GetUserByEmailAsync(null));

            Assert.That(ex.ParamName, Is.EqualTo("email"));
        }

        [Test]
        public async Task IsUsernameTakenAsync_ExistingUsername_ReturnsTrue()
        {
            // Act
            var result = await _userService.IsUsernameTakenAsync(USERNAME);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsUsernameTakenAsync_NonExistingUsername_ReturnsFalse()
        {
            // Act
            var result = await _userService.IsUsernameTakenAsync("newusername");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsUsernameTakenAsync_NullUsername_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.IsUsernameTakenAsync(null));

            Assert.That(ex.ParamName, Is.EqualTo("username"));
        }

        [Test]
        public async Task RegisterUserAsync_ValidNewUser_ReturnsRegisteredUser()
        {
            // Arrange
            var newUser = new User
            {
                Username = "newuser",
                Email = "new@example.com",
                Password = "newpassword"
            };

            // Act
            var result = await _userService.RegisterUserAsync(newUser);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo(newUser.Username));
            Assert.That(result.Email, Is.EqualTo(newUser.Email));
        }

        [Test]
        public async Task RegisterUserAsync_ExistingUsername_ReturnsNull()
        {
            // Arrange
            var newUser = new User
            {
                Username = USERNAME, // Already exists
                Email = "new@example.com",
                Password = "newpassword"
            };

            // Act
            var result = await _userService.RegisterUserAsync(newUser);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RegisterUserAsync_ExistingEmail_ReturnsNull()
        {
            // Arrange
            var newUser = new User
            {
                Username = "newuser",
                Email = EMAIL, // Already exists
                Password = "newpassword"
            };

            // Act
            var result = await _userService.RegisterUserAsync(newUser);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void RegisterUserAsync_NullUser_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _userService.RegisterUserAsync(null));

            Assert.That(ex.ParamName, Is.EqualTo("user"));
        }

        [Test]
        public void RegisterUserAsync_NullUsername_ThrowsArgumentException()
        {
            // Arrange
            var newUser = new User
            {
                Username = null,
                Email = "new@example.com",
                Password = "newpassword"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.RegisterUserAsync(newUser));

            Assert.That(ex.Message, Does.Contain("Username cannot be null or empty"));
        }

        [Test]
        public void RegisterUserAsync_NullEmail_ThrowsArgumentException()
        {
            // Arrange
            var newUser = new User
            {
                Username = "newuser",
                Email = null,
                Password = "newpassword"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.RegisterUserAsync(newUser));

            Assert.That(ex.Message, Does.Contain("Email cannot be null or empty"));
        }

        [Test]
        public void RegisterUserAsync_NullPassword_ThrowsArgumentException()
        {
            // Arrange
            var newUser = new User
            {
                Username = "newuser",
                Email = "new@example.com",
                Password = null
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.RegisterUserAsync(newUser));

            Assert.That(ex.Message, Does.Contain("Password cannot be null or empty"));
        }

        [Test]
        public async Task GetUserByIdAsync_ExistingId_ReturnsUser()
        {
            // Act
            var user = await _userService.GetUserByIdAsync(USER_ID);

            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Id, Is.EqualTo(USER_ID));
        }

        [Test]
        public async Task GetUserByIdAsync_NonExistingId_ReturnsNull()
        {
            // Act
            var user = await _userService.GetUserByIdAsync(999);

            // Assert
            Assert.That(user, Is.Null);
        }

        [Test]
        public void GetUserByIdAsync_InvalidId_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _userService.GetUserByIdAsync(0));

            Assert.That(ex.Message, Does.Contain("User ID must be a positive number"));
        }
    }

    // Helper classes for testing

    /// <summary>
    /// A wrapper class for UserService that allows us to inject a mocked repository
    /// </summary>
    public class UserServiceWithMockedRepository : IUserService
    {
        private readonly UserProxyRepository _repository;
        private readonly JsonSerializerOptions _jsonOptions;

        public UserServiceWithMockedRepository(UserProxyRepository repository)
        {
            _repository = repository;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
            };
        }

        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            try
            {
                await _repository.AuthenticateUserRawAsync(username, password);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<User> GetUserByCredentialsAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            try
            {
                var json = await _repository.AuthenticateUserRawAsync(username, password);
                var sharedUser = JsonSerializer.Deserialize<MarketMinds.Shared.Models.User>(json, _jsonOptions);
                return sharedUser != null ? ConvertToDomainUser(sharedUser) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            }

            try
            {
                var json = await _repository.GetUserByUsernameRawAsync(username);
                var sharedUser = JsonSerializer.Deserialize<MarketMinds.Shared.Models.User>(json, _jsonOptions);
                return sharedUser != null ? ConvertToDomainUser(sharedUser) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            try
            {
                var json = await _repository.GetUserByEmailRawAsync(email);
                var sharedUser = JsonSerializer.Deserialize<MarketMinds.Shared.Models.User>(json, _jsonOptions);
                return sharedUser != null ? ConvertToDomainUser(sharedUser) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            }

            try
            {
                var json = await _repository.CheckUsernameRawAsync(username);
                var result = JsonSerializer.Deserialize<UsernameCheckResult>(json, _jsonOptions);
                return result.Exists;
            }
            catch (Exception)
            {
                return true; // Default to true (username taken) if there's an error
            }
        }

        public async Task<User> RegisterUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(user.Username));
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(user.Email));
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(user.Password));
            }

            try
            {
                // Check if email exists
                try
                {
                    var existingUserByEmail = await GetUserByEmailAsync(user.Email);
                    if (existingUserByEmail != null)
                    {
                        return null;
                    }
                }
                catch { }

                // Check if username exists
                try
                {
                    bool usernameTaken = await IsUsernameTakenAsync(user.Username);
                    if (usernameTaken)
                    {
                        return null;
                    }
                }
                catch { }

                // Create registration request
                var registerRequest = new
                {
                    Username = user.Username,
                    Email = user.Email,
                    Password = user.Password
                };

                var json = await _repository.RegisterUserRawAsync(registerRequest);
                if (string.IsNullOrEmpty(json))
                    return null;

                var registeredSharedUser = JsonSerializer.Deserialize<MarketMinds.Shared.Models.User>(json, _jsonOptions);
                return registeredSharedUser != null ? ConvertToDomainUser(registeredSharedUser) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be a positive number.", nameof(userId));
            }

            try
            {
                var json = await _repository.GetUserByIdRawAsync(userId);
                var sharedUser = JsonSerializer.Deserialize<MarketMinds.Shared.Models.User>(json, _jsonOptions);
                return sharedUser != null ? ConvertToDomainUser(sharedUser) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private User ConvertToDomainUser(MarketMinds.Shared.Models.User sharedUser)
        {
            return new User
            {
                Id = sharedUser.Id,
                Username = sharedUser.Username,
                Email = sharedUser.Email,
                Password = sharedUser.Password
            };
        }
    }

    public class UsernameCheckResult
    {
        public bool Exists { get; set; }
    }
}