using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using System.Net;
using System.Diagnostics;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageRepository messageRepository;
        
        public MessageController(IMessageRepository messageRepository)
        {
            this.messageRepository = messageRepository;
        }
        [HttpPost]
        [ProducesResponseType(typeof(MessageDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateMessage([FromBody] CreateMessageDto createMessageDto)
        {
            var sw = Stopwatch.StartNew();
            if (createMessageDto == null)
            {
                return BadRequest("Request body cannot be null");
            }
            
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                bool isWelcomeMessage = false;
                try 
                {
                    var existingMessages = await messageRepository.GetMessagesByConversationIdAsync(createMessageDto.ConversationId);
                    isWelcomeMessage = existingMessages == null || existingMessages.Count == 0;
                }
                catch (Exception ex)
                {
                    // Silently continue
                }
                
                var createdMessage = await messageRepository.CreateMessageAsync(
                    createMessageDto.ConversationId,
                    createMessageDto.UserId,
                    createMessageDto.Content
                );
                
                var messageDto = new MessageDto
                {
                    Id = createdMessage.Id,
                    ConversationId = createdMessage.ConversationId,
                    UserId = createdMessage.UserId,
                    Content = createdMessage.Content
                };
                
                return CreatedAtAction(nameof(GetMessagesByConversation), new { conversationId = messageDto.ConversationId }, messageDto);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        [HttpGet("conversation/{conversationId}")]
        [ProducesResponseType(typeof(List<MessageDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessagesByConversation(int conversationId)
        {
            try
            {
                var messages = await messageRepository.GetMessagesByConversationIdAsync(conversationId);
                
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
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
