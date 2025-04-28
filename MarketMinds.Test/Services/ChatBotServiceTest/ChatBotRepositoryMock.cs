using System;
using System.Collections.Generic;
using DomainLayer.Domain;
using MarketMinds.Repositories.ChatBotRepository;

namespace MarketMinds.Test.Services.ChatBotServiceTest
{
    public class ChatBotRepositoryMock : IChatBotRepository
    {
        private Node _rootNode;
        private int _loadCount;

        public ChatBotRepositoryMock()
        {
            _loadCount = 0;
            InitializeTestNodes();
        }

        public Node LoadChatTree()
        {
            _loadCount++;
            return _rootNode;
        }

        public int GetLoadCount()
        {
            return _loadCount;
        }

        private void InitializeTestNodes()
        {
            // Create test nodes structure
            _rootNode = new Node
            {
                Id = 1,
                ButtonLabel = "Start",
                LabelText = "Welcome",
                Response = "Welcome to the chat service",
                Children = new List<Node>()
            };

            var option1 = new Node
            {
                Id = 2,
                ButtonLabel = "Option 1",
                LabelText = "Help",
                Response = "How can I help you?",
                Children = new List<Node>()
            };

            var option2 = new Node
            {
                Id = 3,
                ButtonLabel = "Option 2",
                LabelText = "Info",
                Response = "Here's some information",
                Children = new List<Node>()
            };

            var errorNode = new Node
            {
                Id = -1,
                ButtonLabel = "Error",
                LabelText = "Error",
                Response = "Error occurred",
                Children = new List<Node>()
            };

            _rootNode.Children.Add(option1);
            _rootNode.Children.Add(option2);
            _rootNode.Children.Add(errorNode);
        }
    }
}