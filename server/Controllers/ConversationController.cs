using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Repositories.ConversationRepository;
using server.Models;
using System.Net;
using System.Diagnostics;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationRepository conversationRepository;
        
        public ConversationController(IConversationRepository conversationRepository)
        {
            this.conversationRepository = conversationRepository;
            Console.WriteLine("ConversationController constructor called");
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(ConversationDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto createConversationDto)
        {
            var sw = Stopwatch.StartNew();
            Console.WriteLine("\n================================================");
            Console.WriteLine($"CreateConversation called at: {DateTime.Now:HH:mm:ss.fff}");
            Console.WriteLine($"Request data: UserId={createConversationDto?.UserId}");
            
            try
            {
                if (createConversationDto == null)
                {
                    Console.WriteLine("ERROR: createConversationDto is NULL");
                    Console.WriteLine("================================================\n");
                    return BadRequest("Request body cannot be null");
                }
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ERROR: ModelState is invalid");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"- {error.ErrorMessage}");
                    }
                    Console.WriteLine("================================================\n");
                    return BadRequest(ModelState);
                }
                
                Console.WriteLine("Creating conversation entity");
                var conversation = new Conversation
                {
                    UserId = createConversationDto.UserId
                };
                
                Console.WriteLine("Calling repository.CreateConversationAsync");
                var createdConversation = await conversationRepository.CreateConversationAsync(conversation);
                Console.WriteLine($"Conversation created with ID: {createdConversation.Id}");
                
                Console.WriteLine("Creating DTO for response");
                var conversationDto = new ConversationDto
                {
                    Id = createdConversation.Id,
                    UserId = createdConversation.UserId
                };
                
                Console.WriteLine($"Returning successful response with conversation ID: {conversationDto.Id}");
                Console.WriteLine($"Total processing time: {sw.ElapsedMilliseconds}ms");
                Console.WriteLine("================================================\n");
                
                return CreatedAtAction(nameof(GetConversation), new { id = conversationDto.Id }, conversationDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in CreateConversation: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine("================================================\n");
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ConversationDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetConversation(int id)
        {
            Console.WriteLine($"GetConversation called for id: {id}");
            try
            {
                var conversation = await conversationRepository.GetConversationByIdAsync(id);
                
                if (conversation == null)
                {
                    Console.WriteLine($"Conversation with id {id} not found");
                    return NotFound($"Conversation with id {id} not found.");
                }
                
                Console.WriteLine($"Conversation found: Id={conversation.Id}, UserId={conversation.UserId}");
                var conversationDto = new ConversationDto
                {
                    Id = conversation.Id,
                    UserId = conversation.UserId
                };
                
                return Ok(conversationDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetConversation: {ex.Message}");
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(List<ConversationDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserConversations(int userId)
        {
            Console.WriteLine($"GetUserConversations called for userId: {userId}");
            try
            {
                var conversations = await conversationRepository.GetConversationsByUserIdAsync(userId);
                Console.WriteLine($"Found {conversations.Count} conversations for user {userId}");
                
                var conversationDtos = conversations.Select(c => new ConversationDto
                {
                    Id = c.Id,
                    UserId = c.UserId
                }).ToList();
                
                return Ok(conversationDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetUserConversations: {ex.Message}");
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
