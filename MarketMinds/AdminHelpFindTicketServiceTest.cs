/*

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
        private const string SAMPLE_TICKET_ID = "ticket001";
        private const string SAMPLE_USER_NAME = "Test User";
        private const string SAMPLE_DATE = "01-01-2025-12-00";
        private const string SAMPLE_DESCRIPTION = "Test description";
        private const string SAMPLE_CLOSED_STATUS = "false";

        private HelpTicket sampleTicket;

        [SetUp]
        public void Setup()
        {
            mockRepository = new AdminFindHelpTicketRepositoryMock();
            sampleTicket = new HelpTicket
            {
                TicketID = SAMPLE_TICKET_ID,
                UserID = VALID_USER_ID,
                UserName = SAMPLE_USER_NAME,
                DateAndTime = SAMPLE_DATE,
                Description = SAMPLE_DESCRIPTION,
                Closed = SAMPLE_CLOSED_STATUS
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
            var retrievedTickets = mockRepository.LoadTicketsFromDB(ticketIds);

            Assert.That(retrievedTickets, Is.Not.Null);
            Assert.That(retrievedTickets.Count, Is.EqualTo(1));
            Assert.That(retrievedTickets[0].TicketID, Is.EqualTo(SAMPLE_TICKET_ID));
        }

        [Test]
        public void TestGetHelpTicketsByUserId_InvalidUserId_ReturnsEmptyList()
        {
            var ticketIds = mockRepository.GetTicketIDsMatchingCriteria(INVALID_USER_ID);
            var retrievedTickets = mockRepository.LoadTicketsFromDB(ticketIds);

            Assert.That(retrievedTickets, Is.Not.Null);
            Assert.That(retrievedTickets.Count, Is.EqualTo(0));
        }
    }
}
*/