using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ProjetoEventX.DTOs.Chat;
using ProjetoEventX.Models;
using ProjetoEventX.Services;

namespace ProjetoEventX.Controllers.Api
{
    
    [ApiController]
    [Authorize]
    [Route("api/chat")]
    public class ChatApiController : ControllerBase
    {
        private readonly ChatService _chatService;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly IHubContext<NotificationsHub> _notificationsHubContext;

        public ChatApiController(
            ChatService chatService,
            IHubContext<ChatHub> chatHubContext,
            IHubContext<NotificationsHub> notificationsHubContext)
        {
            _chatService = chatService;
            _chatHubContext = chatHubContext;
            _notificationsHubContext = notificationsHubContext;
        }

        [HttpGet("conversations")]
        public async Task<ActionResult<IEnumerable<ChatConversationDto>>> GetConversations([FromQuery] int take = 50)
        {
            var userId = ChatService.GetUserIdFromClaims(User);
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var conversations = await _chatService.GetConversationsAsync(userId.Value, take);
            return Ok(conversations);
        }

        [HttpGet("conversations/{conversationId:int}/messages")]
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetMessages(int conversationId, [FromQuery] int take = 100)
        {
            if (conversationId <= 0)
            {
                return BadRequest(new { message = "ConversationId invalido." });
            }

            var userId = ChatService.GetUserIdFromClaims(User);
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                var messages = await _chatService.GetConversationMessagesAsync(userId.Value, conversationId, take);
                return Ok(messages);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost("messages")]
        public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] SendChatMessageDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (request.ConversationId <= 0)
            {
                return BadRequest(new { message = "ConversationId invalido." });
            }

            var userId = ChatService.GetUserIdFromClaims(User);
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                var message = await _chatService.SendMessageAsync(userId.Value, request.ConversationId, request.Content);

                var groupName = ChatService.GetConversationGroupName(request.ConversationId);
                await _chatHubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", message);
                await _notificationsHubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", message);

                return Ok(message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
