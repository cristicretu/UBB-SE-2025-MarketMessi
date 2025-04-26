using System;
using System.Threading.Tasks;
using NUnit.Framework;
using DomainLayer.Domain;
using MarketMinds.Repositories.UserRepository;
using MarketMinds.Services.RegisterService;

namespace MarketMinds.Test.Services.DreamTeamTests.RegisterServiceTest
{
    [TestFixture]
    public class RegisterServiceTest
    {
        // Constants for test data
        private const string EXISTING_USERNAME = "john_doe";
        private const string EXISTING_EMAIL = "john@example.com";
        private const string NEW_USERNAME = "new_user";
        private const string NEW_EMAIL = "new@example.com";
        private const string EMPTY_STRING = "";
        private const string WHITESPACE = "   ";
        private const string NULL_STRING = null;

        // Constants for error messages
        private const string USERNAME_TAKEN_MESSAGE = "Username is already taken";
        private const string EMAIL_TAKEN_MESSAGE = "Email is already registered";
        private const string REGISTRATION_SUCCESS_MESSAGE = "User registration successful";

        private RegisterService _registerService;
        private UserRepository _userRepository;

        [SetUp]
        public void Setup()
        {
            _userRepository = new UserRepository();
            _registerService = new RegisterService();
        }

        [Test]
        public async Task TestIsUsernameTaken_ExistingUsername_ReturnsTrue()
        {
            // Act
            bool result = await _registerService.IsUsernameTaken(EXISTING_USERNAME);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task TestIsUsernameTaken_NewUsername_ReturnsFalse()
        {
            // Act
            bool result = await _registerService.IsUsernameTaken(NEW_USERNAME);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TestIsUsernameTaken_EmptyUsername_ReturnsFalse()
        {
            // Act
            bool result = await _registerService.IsUsernameTaken(EMPTY_STRING);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TestIsUsernameTaken_WhitespaceUsername_ReturnsFalse()
        {
            // Act
            bool result = await _registerService.IsUsernameTaken(WHITESPACE);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TestIsUsernameTaken_NullUsername_ReturnsFalse()
        {
            // Act
            bool result = await _registerService.IsUsernameTaken(NULL_STRING);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TestRegisterUser_NewUser_ReturnsTrue()
        {
            // Arrange
            var newUser = new User(0, NEW_USERNAME, NEW_EMAIL, "token789")
            {
                UserType = 1,
                Balance = 500f,
                Rating = 0f,
                Password = "secure123"
            };

            // Act
            bool result = await _registerService.RegisterUser(newUser);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task TestRegisterUser_ExistingUsername_ReturnsFalse()
        {
            // Arrange
            var existingUser = new User(0, EXISTING_USERNAME, NEW_EMAIL, "token789")
            {
                UserType = 1,
                Balance = 500f,
                Rating = 0f,
                Password = "secure123"
            };

            // Act
            bool result = await _registerService.RegisterUser(existingUser);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TestRegisterUser_ExistingEmail_ReturnsFalse()
        {
            // Arrange
            var existingEmailUser = new User(0, NEW_USERNAME, EXISTING_EMAIL, "token789")
            {
                UserType = 1,
                Balance = 500f,
                Rating = 0f,
                Password = "secure123"
            };

            // Act
            bool result = await _registerService.RegisterUser(existingEmailUser);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TestRegisterUser_NullUser_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _registerService.RegisterUser(null));
        }

        [Test]
        public async Task TestRegisterUser_EmptyUsername_ReturnsFalse()
        {
            // Arrange
            var emptyUsernameUser = new User(0, EMPTY_STRING, NEW_EMAIL, "token789")
            {
                UserType = 1,
                Balance = 500f,
                Rating = 0f,
                Password = "secure123"
            };

            // Act
            bool result = await _registerService.RegisterUser(emptyUsernameUser);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TestRegisterUser_EmptyEmail_ReturnsFalse()
        {
            // Arrange
            var emptyEmailUser = new User(0, NEW_USERNAME, EMPTY_STRING, "token789")
            {
                UserType = 1,
                Balance = 500f,
                Rating = 0f,
                Password = "secure123"
            };

            // Act
            bool result = await _registerService.RegisterUser(emptyEmailUser);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}