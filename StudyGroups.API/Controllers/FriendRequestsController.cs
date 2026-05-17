using Microsoft.AspNetCore.Mvc;
using StudyGroups.Contracts;
using StudyGroups.Core.Interfaces;
using StudyGroups.Core.Models;

namespace StudyGroups.API.Controllers;

[ApiController]
[Route("api/friend-requests")]
public class FriendRequestsController(IFriendRequestService friendRequestService) : ControllerBase
{
    private readonly IFriendRequestService _friendRequestService = friendRequestService;

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingIncoming([FromQuery] int userId)
    {
        var result = await _friendRequestService.GetPendingIncomingAsync(userId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value.Select(ToDto));
    }

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends([FromQuery] int userId)
    {
        var result = await _friendRequestService.GetFriendIdsAsync(userId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("{id}/respond")]
    public async Task<IActionResult> Respond(int id, RespondFriendRequestRequest request)
    {
        var result = await _friendRequestService.RespondAsync(id, request.ReceiverUserId, request.Accept);

        if (result.IsFailure)
        {
            return result.Error == "Friend request not found"
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        return Ok(ToDto(result.Value));
    }

    [HttpDelete("friends/{friendUserId}")]
    public async Task<IActionResult> RemoveFriend(int friendUserId, [FromQuery] int userId)
    {
        var result = await _friendRequestService.RemoveFriendAsync(userId, friendUserId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFriendRequestRequest request)
    {
        var result = await _friendRequestService.CreateAsync(request.SenderId, request.ReceiverId);

        if (result.IsFailure)
        {
            return result.Error == "Friend request already exists" || result.Error == "Users are already friends"
                ? Conflict(result.Error)
                : BadRequest(result.Error);
        }

        return Ok(ToDto(result.Value));
    }

    private static FriendRequestDto ToDto(FriendRequest request)
    {
        return new FriendRequestDto
        {
            Id = request.Id,
            RequesterUserId = request.RequesterUserId,
            RequesterUsername = request.RequesterUsername,
            ReceiverUserId = request.ReceiverUserId,
            ReceiverUsername = request.ReceiverUsername,
            Status = request.Status,
            CreatedAt = request.CreatedAt,
            RespondedAt = request.RespondedAt
        };
    }
}
