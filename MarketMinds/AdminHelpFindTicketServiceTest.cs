using System.Collections.Generic;
using NUnit.Framework;
using MarketMinds.Models;  
using MarketMinds.Test.Services.DreamTeamTests.AdminHelpFindTicketService;

namespace MarketMinds.Test.Services.DreamTeamTests.AdminHelpFindTicketService
{
    [TestFixture]
    public class AdminHelpFindTicketServiceTest
    {
        private AdminFindHelpTicketRepositoryMock mockRepository;

        private const string VALID_USER_ID = "user123";
        private const string INVALID_USER_ID = "invalidUser";

        private HelpTicket sampleTicket;

        [SetUp]
        public void Setup()
        {
            mockRepository = new AdminFindHelpTicketRepositoryMock();

            sampleTicket = new HelpTicket
            {
                TicketID = "ticket001",
                UserID = VALID_USER_ID,
                UserName = "Test User",
                DateAndTime = "01-01-2025-12-00",
                Description = "Test description",
                Closed = "false"
            };

            mockRepository.AddHelpTicket(sampleTicket);
        }

        [Test]
        public void TestDoesUserIdExist_ExistingUserId_ReturnsTrue()
        {
            bool result = mockRepository.DoesUserIDExist(VALID_USER_ID);
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestDoesUserIdExist_NonExistingUserId_ReturnsFalse()
        {
            bool result = mockRepository.DoesUserIDExist(INVALID_USER_ID);
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestGetHelpTicketsByUserId_ValidUserId_ReturnsTickets()
        {
            var ticketIds = mockRepository.GetTicketIDsMatchingCriteria(VALID_USER_ID);
            var tickets = mockRepository.LoadTicketsFromDB(ticketIds);

            Assert.IsNotNull(tickets);
            Assert.AreEqual(1, tickets.Count);
            Assert.AreEqual(sampleTicket.TicketID, tickets[0].TicketID);
        }

        [Test]
        public void TestGetHelpTicketsByUserId_InvalidUserId_ReturnsEmptyList()
        {
            var ticketIds = mockRepository.GetTicketIDsMatchingCriteria(INVALID_USER_ID);
            var tickets = mockRepository.LoadTicketsFromDB(ticketIds);

            Assert.IsNotNull(tickets);
            Assert.AreEqual(0, tickets.Count);
        }
    }
}
