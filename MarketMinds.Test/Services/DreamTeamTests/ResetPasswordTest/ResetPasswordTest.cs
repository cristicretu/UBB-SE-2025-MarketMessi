using System;
using NUnit.Framework;
using DomainLayer.Domain;
using MarketMinds.Repositories.UserRepository;
using MarketMinds.Services.ResetPasswordService;

namespace MarketMinds.Test.Services.DreamTeamTests.ResetPasswordTest
{
    [TestFixture]
    public class ResetPasswordServiceTest
    {
        // Constants for test data
        private const string VALID_EMAIL = "john@example.com";
        private const string NEW_PASSWORD = "newSecurePassword123";
        private const string INVALID_EMAIL = "nonexistent@example.com";
        private const string EMPTY_STRING = "";
        private const string WHITESPACE = "   ";
        private const string NULL_STRING = null;

        private ResetPasswordService _resetPasswordService;
        private UserRepository _userRepository;

        [SetUp]
        public void Setup()
        {
            _userRepository = new UserRepository();
            _resetPasswordService = new ResetPasswordService();
        }

        [Test]
        public void TestResetPassword_ValidEmail_ReturnsTrue()
        {
            // Act
            bool result = _resetPasswordService.ResetPassword(VALID_EMAIL, NEW_PASSWORD);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestResetPassword_InvalidEmail_ReturnsFalse()
        {
            // Act
            bool result = _resetPasswordService.ResetPassword(INVALID_EMAIL, NEW_PASSWORD);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestResetPassword_EmptyEmail_ReturnsFalse()
        {
            // Act
            bool result = _resetPasswordService.ResetPassword(EMPTY_STRING, NEW_PASSWORD);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestResetPassword_WhitespaceEmail_ReturnsFalse()
        {
            // Act
            bool result = _resetPasswordService.ResetPassword(WHITESPACE, NEW_PASSWORD);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestResetPassword_EmptyPassword_ReturnsFalse()
        {
            // Act
            bool result = _resetPasswordService.ResetPassword(VALID_EMAIL, EMPTY_STRING);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestResetPassword_WhitespacePassword_ReturnsFalse()
        {
            // Act
            bool result = _resetPasswordService.ResetPassword(VALID_EMAIL, WHITESPACE);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestResetPassword_NullEmail_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _resetPasswordService.ResetPassword(NULL_STRING, NEW_PASSWORD));
        }

        [Test]
        public void TestResetPassword_NullPassword_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _resetPasswordService.ResetPassword(VALID_EMAIL, NULL_STRING));
        }
    }
}