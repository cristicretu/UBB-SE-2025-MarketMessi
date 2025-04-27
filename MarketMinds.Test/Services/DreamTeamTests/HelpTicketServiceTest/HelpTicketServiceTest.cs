using System;
using System.Collections.Generic;
using NUnit.Framework;
using MarketMinds.Models;
using Marketplace_SE.Services;

namespace MarketMinds.Test.Services.DreamTeamTests.HelpTicketService
{
    [TestFixture]
    public class HelpTicketServiceTest
    {
        private Marketplace_SE.Services.HelpTicketService helpTicketService;
        private HelpTicketRepositoryMock helpTicketRepositoryMock;

        private const string VALID_USER_ID = "user123";
        private const string VALID_USERNAME = "TestUser";
        private const string VALID_DESCRIPTION = "Test Description";
        private const string VALID_TICKET_ID_STRING = "1";
        private const int VALID_TICKET_ID_INT = 1;

        [SetUp]
        public void Setup()
        {
            helpTicketRepositoryMock = new HelpTicketRepositoryMock();
            helpTicketService = new Marketplace_SE.Services.HelpTicketService(helpTicketRepositoryMock);
        }

        [Test]
        public void TestValidateUser_ExistingUser_ReturnsTrue()
        {
            helpTicketRepositoryMock.AddMockUserId(VALID_USER_ID);

            bool result = helpTicketService.ValidateUser(VALID_USER_ID);

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestValidateUser_NonExistingUser_ReturnsFalse()
        {
            bool result = helpTicketService.ValidateUser("nonexistent");

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestCreateHelpTicket_ValidInput_AddsTicket()
        {
            int result = helpTicketService.CreateHelpTicket(VALID_USER_ID, VALID_USERNAME, VALID_DESCRIPTION);

            Assert.That(result, Is.EqualTo(1));
            Assert.That(helpTicketRepositoryMock.GetAddedTickets().Count, Is.EqualTo(1));
        }

        [Test]
        public void TestGetTicketsForUser_ReturnsTickets()
        {
            helpTicketRepositoryMock.AddMockTicket(new HelpTicket
            {
                TicketID = VALID_TICKET_ID_STRING,
                UserID = VALID_USER_ID,
                UserName = VALID_USERNAME,
                DateAndTime = DateTime.Now.ToString("dd-MM-yyyy-HH-mm"),
                Description = VALID_DESCRIPTION,
                Closed = "No"
            });

            List<HelpTicket> retrievedTickets = helpTicketService.GetTicketsForUser(VALID_USER_ID);

            Assert.That(retrievedTickets, Is.Not.Null);
            Assert.That(retrievedTickets.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestValidateTicketID_ValidTicketID_ReturnsTrue()
        {
            helpTicketRepositoryMock.AddMockTicketId(VALID_TICKET_ID_INT);

            bool result = helpTicketService.ValidateTicketID(VALID_TICKET_ID_INT);

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestUpdateTicketDescription_UpdatesSuccessfully()
        {
            int result = helpTicketService.UpdateTicketDescription(VALID_TICKET_ID_STRING, "Updated Description");

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void TestCloseTicket_ClosesSuccessfully()
        {
            int result = helpTicketService.CloseTicket(VALID_TICKET_ID_STRING);

            Assert.That(result, Is.EqualTo(1));
        }
    }
}
