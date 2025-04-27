using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Repositories.MessageRepository;
using server.Models;
using System.Net;
using System.Diagnostics;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageRepository messageRepository;
        
        public MessageController(IMessageRepository messageRepository)
        {
            this.messageRepository = messageRepository;
            Console.WriteLine("MessageController constructor called");
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(MessageDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateMessage([FromBody] CreateMessageDto createMessageDto)
        {
            var sw = Stopwatch.StartNew();
            Console.WriteLine("\n================================================");
            Console.WriteLine($"CreateMessage called at: {DateTime.Now:HH:mm:ss.fff}");
            
            if (createMessageDto == null)
            {
                Console.WriteLine("ERROR: createMessageDto is NULL");
                Console.WriteLine("================================================\n");
                return BadRequest("Request body cannot be null");
            }
            
            Console.WriteLine($"Request data: ConversationId={createMessageDto.ConversationId}, " +
                             $"UserId={createMessageDto.UserId}, " +
                             $"Content={createMessageDto.Content?.Substring(0, Math.Min(30, createMessageDto.Content?.Length ?? 0))}...");
            
            try
            {
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
                
                // Check if this is a welcome message (first message in conversation)
                bool isWelcomeMessage = false;
                try 
                {
                    var existingMessages = await messageRepository.GetMessagesByConversationIdAsync(createMessageDto.ConversationId);
                    isWelcomeMessage = existingMessages == null || existingMessages.Count == 0;
                    Console.WriteLine($"Is welcome message: {isWelcomeMessage} (found {existingMessages?.Count ?? 0} existing messages)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Error checking if welcome message: {ex.Message}");
                }
                
                Console.WriteLine("Creating message entity");
                var message = new Message
                {
                    ConversationId = createMessageDto.ConversationId,
                    UserId = createMessageDto.UserId,
                    Content = createMessageDto.Content
                };
                
                Console.WriteLine("Calling repository.CreateMessageAsync");
                var createdMessage = await messageRepository.CreateMessageAsync(message);
                Console.WriteLine($"Message created with ID: {createdMessage.Id}");
                
                Console.WriteLine("Creating DTO for response");
                var messageDto = new MessageDto
                {
                    Id = createdMessage.Id,
                    ConversationId = createdMessage.ConversationId,
                    UserId = createdMessage.UserId,
                    Content = createdMessage.Content
                };
                
                Console.WriteLine($"Returning successful response with message ID: {messageDto.Id}");
                Console.WriteLine($"Total processing time: {sw.ElapsedMilliseconds}ms");
                Console.WriteLine("================================================\n");
                
                return CreatedAtAction(nameof(GetMessagesByConversation), new { conversationId = messageDto.ConversationId }, messageDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in CreateMessage: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine("================================================\n");
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        [HttpGet("conversation/{conversationId}")]
        [ProducesResponseType(typeof(List<MessageDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessagesByConversation(int conversationId)
        {
            Console.WriteLine($"GetMessagesByConversation called for conversationId: {conversationId}");
            try
            {
                var messages = await messageRepository.GetMessagesByConversationIdAsync(conversationId);
                Console.WriteLine($"Found {messages.Count} messages for conversation {conversationId}");
                
                var messageDtos = messages.Select(m => new MessageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    UserId = m.UserId,
                    Content = m.Content
                }).ToList();
                
                return Ok(messageDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetMessagesByConversation: {ex.Message}");
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
