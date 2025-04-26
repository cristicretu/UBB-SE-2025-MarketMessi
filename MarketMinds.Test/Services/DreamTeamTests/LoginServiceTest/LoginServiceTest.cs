using System;
using NUnit.Framework;
using DomainLayer.Domain;
using MarketMinds.Repositories.UserRepository;
using MarketMinds.Services.LoginService;

namespace MarketMinds.Test.Services.DreamTeamTests.LoginServiceTest
{
    [TestFixture]
    public class LoginServiceTest
    {
        // Constants for test data
        private const string VALID_USERNAME = "john_doe";
        private const string VALID_PASSWORD = "1234";
        private const string INVALID_USERNAME = "nonexistent_user";
        private const string INVALID_PASSWORD = "wrong_password";
        private const string EMPTY_STRING = "";
        private const string WHITESPACE = "   ";
        private const string NULL_STRING = null;

        private LoginService _loginService;
        private UserRepository _userRepository;

        [SetUp]
        public void Setup()
        {
            _userRepository = new UserRepository();
            _loginService = new LoginService();
        }

        [Test]
        public void TestAuthenticateUser_ValidCredentials_ReturnsTrue()
        {
            // Act
            bool result = _loginService.AuthenticateUser(VALID_USERNAME, VALID_PASSWORD);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestAuthenticateUser_InvalidUsername_ReturnsFalse()
        {
            // Act
            bool result = _loginService.AuthenticateUser(INVALID_USERNAME, VALID_PASSWORD);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestAuthenticateUser_InvalidPassword_ReturnsFalse()
        {
            // Act
            bool result = _loginService.AuthenticateUser(VALID_USERNAME, INVALID_PASSWORD);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestAuthenticateUser_EmptyUsername_ReturnsFalse()
        {
            // Act
            bool result = _loginService.AuthenticateUser(EMPTY_STRING, VALID_PASSWORD);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestAuthenticateUser_EmptyPassword_ReturnsFalse()
        {
            // Act
            bool result = _loginService.AuthenticateUser(VALID_USERNAME, EMPTY_STRING);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestAuthenticateUser_WhitespaceCredentials_ReturnsFalse()
        {
            // Act
            bool result = _loginService.AuthenticateUser(WHITESPACE, WHITESPACE);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestGetUserByCredentials_ValidCredentials_ReturnsUser()
        {
            // Act
            User result = _loginService.GetUserByCredentials(VALID_USERNAME, VALID_PASSWORD);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.EqualTo(VALID_USERNAME));
        }

        [Test]
        public void TestGetUserByCredentials_InvalidCredentials_ReturnsNull()
        {
            // Act
            User result = _loginService.GetUserByCredentials(INVALID_USERNAME, INVALID_PASSWORD);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetUserByCredentials_NullUsername_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _loginService.GetUserByCredentials(NULL_STRING, VALID_PASSWORD));
        }

        [Test]
        public void TestGetUserByCredentials_NullPassword_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _loginService.GetUserByCredentials(VALID_USERNAME, NULL_STRING));
        }
    }
}