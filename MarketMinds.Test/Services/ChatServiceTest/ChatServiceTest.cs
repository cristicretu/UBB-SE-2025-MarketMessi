using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MarketMinds.Shared.Models;
using MarketMinds.Repositories.ChatRepository;
using MarketMinds.Shared.Services.DreamTeam.ChatService;
using MarketMinds.Test.Services.ChatServiceTest;

namespace MarketMinds.Tests.Services.ChatServiceTest
{
    [TestFixture]
    public class ChatServiceTest
    {
        private ChatService _chatService;
        private ChatRepositoryMock _chatRepositoryMock;
        private User _currentUser;
        private User _targetUser;
        private Conversation _testConversation;
        private List<Message> _testMessages;

        // Constants
        private const int CURRENT_USER_ID = 1;
        private const int TARGET_USER_ID = 2;
        private const int CONVERSATION_ID = 1;
        private const string TEST_MESSAGE_CONTENT = "Hello";
        private const long TEST_TIMESTAMP = 1234567890;

        [SetUp]
        public void Setup()
        {
            _chatRepositoryMock = new ChatRepositoryMock();
            _chatService = new ChatService(_chatRepositoryMock);

            _currentUser = new User(CURRENT_USER_ID, "Current User", "current@test.com");
            _targetUser = new User(TARGET_USER_ID, "Target User", "target@test.com");

            _testConversation = new Conversation
            {
                Id = CONVERSATION_ID,
                UserId1 = CURRENT_USER_ID,
                UserId2 = TARGET_USER_ID
            };

            _testMessages = new List<Message>
            {
                new Message
                {
                    Id = 1,
                    ConversationId = CONVERSATION_ID,
                    Creator = CURRENT_USER_ID,
                    Timestamp = TEST_TIMESTAMP - 1000,
                    ContentType = "text",
                    Content = "First message"
                },
                new Message
                {
                    Id = 2,
                    ConversationId = CONVERSATION_ID,
                    Creator = TARGET_USER_ID,
                    Timestamp = TEST_TIMESTAMP,
                    ContentType = "text",
                    Content = "Second message"
                }
            };
        }

        [Test]
        public void TestInitialize_NewConversation_CreatesConversation()
        {
            // Act
            _chatService.Initialize(_currentUser, _targetUser);

            // Assert
            Assert.That(_chatRepositoryMock.CreateConversationCount, Is.EqualTo(1));
        }

        [Test]
        public void TestInitialize_ExistingConversation_RetrievesConversation()
        {
            // Arrange
            _chatRepositoryMock.AddTestConversation(_testConversation);

            // Act
            _chatService.Initialize(_currentUser, _targetUser);

            // Assert
            Assert.That(_chatRepositoryMock.GetConversationCount, Is.EqualTo(1));
        }

        [Test]
        public void TestInitialize_LoadsInitialMessages()
        {
            // Arrange
            _chatRepositoryMock.AddTestConversation(_testConversation);
            _chatRepositoryMock.AddTestMessages(_testMessages);

            // Act
            _chatService.Initialize(_currentUser, _targetUser);

            // Assert
            Assert.That(_chatService.GetInitialMessages().Count, Is.EqualTo(2));
        }

        [Test]
        public void TestSendTextMessage_ValidMessage_ReturnsTrue()
        {
            // Arrange
            _chatService.Initialize(_currentUser, _targetUser);

            // Act
            var result = _chatService.SendTextMessage(TEST_MESSAGE_CONTENT);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestSendTextMessage_ValidMessage_AddsToRepository()
        {
            // Arrange
            _chatService.Initialize(_currentUser, _targetUser);

            // Act
            _chatService.SendTextMessage(TEST_MESSAGE_CONTENT);

            // Assert
            Assert.That(_chatRepositoryMock.AddMessageCount, Is.EqualTo(1));
        }

        [Test]
        public void TestSendTextMessage_EmptyMessage_ReturnsFalse()
        {
            // Arrange
            _chatService.Initialize(_currentUser, _targetUser);

            // Act
            var result = _chatService.SendTextMessage("");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestSendImageMessage_ValidImage_ReturnsTrue()
        {
            // Arrange
            _chatService.Initialize(_currentUser, _targetUser);
            var imageData = new byte[] { 0x01, 0x02, 0x03 };

            // Act
            var result = _chatService.SendImageMessage(imageData);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestSendImageMessage_EmptyImage_ReturnsFalse()
        {
            // Arrange
            _chatService.Initialize(_currentUser, _targetUser);

            // Act
            var result = _chatService.SendImageMessage(Array.Empty<byte>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestCheckForNewMessages_NoNewMessages_ReturnsEmptyList()
        {
            // Arrange
            _chatService.Initialize(_currentUser, _targetUser);

            // Act
            var result = _chatService.CheckForNewMessages();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void TestCheckForNewMessages_WithNewMessages_ReturnsMessages()
        {
            // Arrange
            _chatRepositoryMock.AddTestConversation(_testConversation);
            _chatService.Initialize(_currentUser, _targetUser);
            _chatRepositoryMock.AddTestMessages(new List<Message>
            {
                new Message
                {
                    ConversationId = CONVERSATION_ID,
                    Creator = TARGET_USER_ID,
                    Timestamp = TEST_TIMESTAMP + 1000,
                    ContentType = "text",
                    Content = "New message"
                }
            });

            // Act
            var result = _chatService.CheckForNewMessages();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestGetConversation_ReturnsCurrentConversation()
        {
            // Arrange
            _chatRepositoryMock.AddTestConversation(_testConversation);
            _chatService.Initialize(_currentUser, _targetUser);

            // Act
            var result = _chatService.GetConversation();

            // Assert
            Assert.That(result?.Id, Is.EqualTo(CONVERSATION_ID));
        }
    }
}