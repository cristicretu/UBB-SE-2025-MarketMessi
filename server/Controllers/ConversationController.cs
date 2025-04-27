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
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(ConversationDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto createConversationDto)
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                if (createConversationDto == null)
                {
                    return BadRequest("Request body cannot be null");
                }
                
                if (!ModelState.IsValid)
                {
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"- {error.ErrorMessage}");
                    }
                    return BadRequest(ModelState);
                }
                
                var conversation = new Conversation
                {
                    UserId = createConversationDto.UserId
                };
                
                var createdConversation = await conversationRepository.CreateConversationAsync(conversation);
                
                var conversationDto = new ConversationDto
                {
                    Id = createdConversation.Id,
                    UserId = createdConversation.UserId
                };
                
                return CreatedAtAction(nameof(GetConversation), new { id = conversationDto.Id }, conversationDto);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ConversationDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetConversation(int id)
        {
            try
            {
                var conversation = await conversationRepository.GetConversationByIdAsync(id);
                
                if (conversation == null)
                {
                    return NotFound($"Conversation with id {id} not found.");
                }
                
                var conversationDto = new ConversationDto
                {
                    Id = conversation.Id,
                    UserId = conversation.UserId
                };
                
                return Ok(conversationDto);
            }
            catch (Exception ex)
            {
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
                
                var conversationDtos = conversations.Select(c => new ConversationDto
                {
                    Id = c.Id,
                    UserId = c.UserId
                }).ToList();
                
                return Ok(conversationDtos);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
