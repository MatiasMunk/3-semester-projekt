using Microsoft.AspNetCore.Mvc;
using StudyGroups.Contracts;
using StudyGroups.Core.Interfaces;
using StudyGroups.Core.Models;

namespace StudyGroups.API.Controllers;

[ApiController]
[Route("api/private-messages")]
public class PrivateMessagesController(IPrivateMessageService messageService) : ControllerBase
{
    private readonly IPrivateMessageService _messageService = messageService;

    [HttpGet]
    public async Task<IActionResult> GetConversation([FromQuery] int sessionId, [FromQuery] int userId, [FromQuery] int otherUserId)
    {
        var result = await _messageService.GetConversationAsync(sessionId, userId, otherUserId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value.Select(ToDto));
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread([FromQuery] int sessionId, [FromQuery] int userId)
    {
        var result = await _messageService.GetUnreadAsync(sessionId, userId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value.Select(ToDto));
    }

    [HttpPost]
    public async Task<IActionResult> Send(SendPrivateMessageRequest request)
    {
        var result = await _messageService.SendAsync(new PrivateMessage
        {
            SessionId = request.SessionId,
            SenderUserId = request.SenderUserId,
            ReceiverUserId = request.ReceiverUserId,
            Message = request.Message
        });

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(ToDto(result.Value));
    }

    private static PrivateMessageDto ToDto(PrivateMessage message)
    {
        return new PrivateMessageDto
        {
            Id = message.Id,
            SessionId = message.SessionId,
            SenderUserId = message.SenderUserId,
            SenderUsername = message.SenderUsername,
            ReceiverUserId = message.ReceiverUserId,
            ReceiverUsername = message.ReceiverUsername,
            Message = message.Message,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };
    }
}
