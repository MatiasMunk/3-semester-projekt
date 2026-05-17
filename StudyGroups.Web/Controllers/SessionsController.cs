using Microsoft.AspNetCore.Mvc;
using StudyGroups.Contracts;
using StudyGroups.Http.Interfaces;
using StudyGroups.Web.Extensions;
using StudyGroups.Web.Services;

namespace StudyGroups.Web.Controllers;

public class SessionsController : Controller
{
    private readonly IStudySessionApi _api;
    private readonly ICategoryApi _categoryApi;
    private readonly IPrivateMessageApi _privateMessages;
    private readonly IFriendRequestApi _friendRequests;
    private readonly LiveKitTokenService _liveKit;

    public SessionsController(
        IStudySessionApi api,
        ICategoryApi categoryApi,
        IPrivateMessageApi privateMessages,
        IFriendRequestApi friendRequests,
        LiveKitTokenService liveKit)
    {
        _api = api;
        _categoryApi = categoryApi;
        _privateMessages = privateMessages;
        _friendRequests = friendRequests;
        _liveKit = liveKit;
    }

    // -----------------------------
    // LIST
    // -----------------------------
    public async Task<IActionResult> Index(string? category)
    {
        Console.WriteLine($"WEB CATEGORY: '{category}'");

        var sessions = await _api.GetAll(category);

        ViewBag.UserId = HttpContext.GetUserId();
        ViewBag.Category = category;

        return View(sessions);
    }

    // -----------------------------
    // CREATE VIEW
    // -----------------------------
    public async Task<IActionResult> Create()
    {
        var userId = HttpContext.GetUserId();

        if (userId == null)
        {
            TempData["Error"] = "You must log in before creating a session.";
            return RedirectToAction("Index");
        }

        await LoadCategoriesAsync();
        return View(new CreateStudySessionRequest
        {
            StartTime = DateTime.Now.AddHours(1),
            MaxParticipants = 4,
            UserId = userId.Value
        });
    }

    // -----------------------------
    // CREATE POST
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> Create(CreateStudySessionRequest request)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null)
        {
            ModelState.AddModelError("", "You must log in before creating a session.");
        }
        else
        {
            // Do not trust UserId from the browser/form. The creator must come
            // from the logged-in session, otherwise SQL rejects CreatedByUserId.
            request.UserId = userId.Value;
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return View(request);
        }

        try
        {
            await _api.Create(request);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await LoadCategoriesAsync();
            return View(request);
        }
    }

    // -----------------------------
    // JOIN
    // -----------------------------
    public async Task<IActionResult> Join(int id)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null)
            return RedirectToAction("Login", "Account");

        try
        {
            await _api.Join(id, userId.Value);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index");
    }

    // -----------------------------
    // LIVEKIT ROOM
    // -----------------------------
    public async Task<IActionResult> Room(int id)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null)
        {
            TempData["Error"] = "You must log in before entering a live room.";
            return RedirectToAction("Index");
        }

        var session = await _api.GetById(id);

        if (session == null)
            return NotFound();

        ViewBag.SessionId = id;
        ViewBag.RoomName = BuildLiveKitRoomName(id);
        ViewBag.LiveKitConfigured = _liveKit.IsConfigured;
        ViewBag.LiveKitUrl = _liveKit.ServerUrl;
        ViewBag.DisplayName = HttpContext.GetDisplayName();
        ViewBag.UserId = userId.Value;

        return View(session);
    }

    [HttpGet]
    public async Task<IActionResult> Notifications(int sessionId)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null)
            return Unauthorized("You must log in to read notifications.");

        var friendRequests = await _friendRequests.GetPendingIncoming(userId.Value);
        var unreadMessages = await _privateMessages.GetUnread(sessionId, userId.Value);
        var friendUserIds = await _friendRequests.GetFriendIds(userId.Value);

        return Ok(new RoomNotificationsDto
        {
            PendingFriendRequests = friendRequests,
            UnreadPrivateMessages = unreadMessages,
            FriendUserIds = friendUserIds
        });
    }

    [HttpGet]
    public async Task<IActionResult> PrivateMessages(int sessionId, int otherUserId)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null)
            return Unauthorized("You must log in to read private messages.");

        var messages = await _privateMessages.GetConversation(sessionId, userId.Value, otherUserId);
        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> PrivateMessages([FromBody] SendPrivateMessageRequest request)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null)
            return Unauthorized("You must log in to send private messages.");

        request.SenderUserId = userId.Value;

        var message = await _privateMessages.Send(request);
        return Ok(message);
    }

    [HttpPost]
    public async Task<IActionResult> FriendRequest([FromBody] CreateFriendRequestRequest request)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null)
            return Unauthorized("You must log in to send friend requests.");

        request.ReceiverId = userId.Value;

        var friendRequest = await _friendRequests.Create(request);
        return Ok(friendRequest);
    }

    [HttpDelete]
    public async Task<IActionResult> Friend(int id)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null)
            return Unauthorized("You must log in to remove friends.");

        await _friendRequests.RemoveFriend(userId.Value, id);
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> FriendRequestResponse(int id, [FromBody] RespondFriendRequestRequest request)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null)
            return Unauthorized("You must log in to respond to friend requests.");

        request.ReceiverUserId = userId.Value;

        var friendRequest = await _friendRequests.Respond(id, request);
        return Ok(friendRequest);
    }

    [HttpPost]
    public IActionResult LiveKitToken(int id)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null)
            return Unauthorized("You must log in before entering a live room.");

        var roomName = BuildLiveKitRoomName(id);
        var identity = $"user-{userId.Value}";
        var displayName = HttpContext.GetDisplayName();
        var token = _liveKit.CreateRoomToken(roomName, identity, displayName);

        return Ok(new
        {
            url = _liveKit.ServerUrl,
            token,
            room = roomName,
            identity,
            displayName
        });
    }

    private async Task LoadCategoriesAsync()
    {
        ViewBag.Categories = (await _categoryApi.GetAll()).ToList();
    }

    private static string BuildLiveKitRoomName(int sessionId)
    {
        return $"study-session-{sessionId}";
    }
}
