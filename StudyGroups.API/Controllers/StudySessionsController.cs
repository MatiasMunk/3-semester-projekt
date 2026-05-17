using Microsoft.AspNetCore.Mvc;
using StudyGroups.Core.Interfaces;
using StudyGroups.Core.Models;
using StudyGroups.Contracts;

namespace StudyGroups.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudySessionsController(IStudySessionService sessionService) : ControllerBase
{
    private readonly IStudySessionService _sessionService = sessionService;

    // -----------------------------
    // GET: api/studysessions
    // -----------------------------
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category)
    {
        var result = await _sessionService.GetAllAsync(category);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var dto = result.Value.Select(s => new StudySessionDto
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,

            TopicId = s.TopicId,
            TopicName = s.Topic?.Name ?? "",
            TopicIcon = s.Topic?.Icon ?? "",

            Location = s.Location,
            StartTime = s.StartTime,
            MaxParticipants = s.MaxParticipants,
            CurrentParticipants = s.CurrentParticipants,
            CreatedByUserId = s.CreatedByUserId
        });

        return Ok(dto);
    }

    // -----------------------------
    // GET: api/studysessions/{id}
    // -----------------------------
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _sessionService.GetByIdAsync(id);

        if (result.IsFailure)
            return NotFound(result.Error);

        var s = result.Value;

        var dto = new StudySessionDto
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            TopicId = s.TopicId,
            TopicName = s.Topic?.Name ?? "",
            TopicIcon = s.Topic?.Icon ?? "",
            Location = s.Location,
            StartTime = s.StartTime,
            MaxParticipants = s.MaxParticipants,
            CurrentParticipants = s.CurrentParticipants,
            CreatedByUserId = s.CreatedByUserId,

            Participants = s.Participants?
                .Select(p => new SessionParticipantDto
                {
                    UserId = p.UserId,
                    Username = p.User?.Username
                })
                .ToList()
        };

        return Ok(dto);
    }

    // -----------------------------
    // POST: api/studysessions
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> Create(CreateStudySessionRequest request)
    {
        var session = new StudySession
        {
            Title = request.Title,
            Description = request.Description,

            TopicId = request.TopicId,

            Location = request.Location,
            StartTime = request.StartTime,
            MaxParticipants = request.MaxParticipants,

            CreatedByUserId = request.UserId
        };

        var result = await _sessionService.CreateAsync(session);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var dto = new StudySessionDto
        {
            Id = result.Value.Id,
            Title = result.Value.Title,
            Description = result.Value.Description,

            TopicId = result.Value.TopicId,
            TopicName = result.Value.Topic?.Name ?? "",
            TopicIcon = result.Value.Topic?.Icon ?? "",

            Location = result.Value.Location,
            StartTime = result.Value.StartTime,
            MaxParticipants = result.Value.MaxParticipants,
            CurrentParticipants = result.Value.CurrentParticipants,
            CreatedByUserId = result.Value.CreatedByUserId
        };

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    // -----------------------------
    // PUT: api/studysessions/{id}
    // -----------------------------
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateStudySessionRequest request)
    {
        var session = new StudySession
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,

            TopicId = request.TopicId,

            Location = request.Location,
            StartTime = request.StartTime,
            MaxParticipants = request.MaxParticipants
        };

        var result = await _sessionService.UpdateAsync(session);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    // -----------------------------
    // DELETE: api/studysessions/{id}
    // -----------------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _sessionService.DeleteAsync(id);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    // -----------------------------
    // POST: api/studysessions/{id}/join
    // -----------------------------
    [HttpPost("{id}/join")]
    public async Task<IActionResult> Join(int id, JoinSessionRequest request)
    {
        var result = await _sessionService.JoinSessionAsync(id, request.UserId);

        if (result.IsFailure)
        {
            return result.Error switch
            {
                "Session not found" => NotFound(result.Error),
                "Session is full" => Conflict(result.Error),
                "User already joined" => Conflict(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return NoContent();
    }

    // -----------------------------
    // POST: api/studysessions/{id}/leave
    // -----------------------------
    [HttpPost("{id}/leave")]
    public async Task<IActionResult> Leave(int id, LeaveSessionRequest request)
    {
        var result = await _sessionService.LeaveSessionAsync(id, request.UserId);

        if (result.IsFailure)
        {
            return result.Error switch
            {
                "User is not part of session" => Conflict(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return NoContent();
    }
}