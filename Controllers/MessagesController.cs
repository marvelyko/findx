using Microsoft.AspNetCore.Mvc;
using FindX.Services;
using FindX.Models;

namespace FindX.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IWebHostEnvironment _environment;

    public MessagesController(IMessageService messageService, IWebHostEnvironment environment)
    {
        _messageService = messageService;
        _environment = environment;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitMessage([FromForm] SubmitMessageRequest request)
    {
        if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.MessageText))
        {
            return BadRequest("Name and message are required.");
        }

        string photoPath = string.Empty;
        if (request.Photo != null && request.Photo.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.Photo.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Photo.CopyToAsync(stream);
            }

            photoPath = $"/uploads/{uniqueFileName}";
        }

        var message = await _messageService.CreateMessageAsync(
            request.Name,
            request.MessageText,
            photoPath,
            request.Latitude,
            request.Longitude,
            request.SessionId ?? Guid.NewGuid().ToString()
        );

        return Ok(new { messageId = message.Id, sessionId = message.SessionId });
    }

    [HttpGet("others/{sessionId}")]
    public async Task<IActionResult> GetOtherMessages(string sessionId)
    {
        var messages = await _messageService.GetOtherMessagesAsync(sessionId);
        return Ok(messages.Select(m => new
        {
            m.Id,
            m.Name,
            m.MessageText,
            m.PhotoPath,
            m.CreatedAt,
            HasConsent = m.Consents.Any(c => c.ViewerSessionId == sessionId),
            HasMutualConsent = m.Consents.Any(c => c.ViewerSessionId == sessionId && c.HasMutualConsent)
        }));
    }

    [HttpPost("request-view")]
    public async Task<IActionResult> RequestView([FromBody] RequestViewRequest request)
    {
        var result = await _messageService.RequestViewAsync(request.MessageId, request.ViewerSessionId);
        
        if (result)
        {
            // Check if mutual consent now exists
            var message = await _messageService.GetMessageAsync(request.MessageId);
            if (message != null)
            {
                var hasMutualConsent = await _messageService.CheckMutualConsentAsync(message.SessionId, request.ViewerSessionId);
                return Ok(new { success = true, hasMutualConsent });
            }
        }

        return Ok(new { success = result, hasMutualConsent = false });
    }

    [HttpDelete("{id}/{sessionId}")]
    public async Task<IActionResult> DeleteMessage(int id, string sessionId)
    {
        await _messageService.DeleteMessageAsync(id, sessionId);
        return Ok(new { success = true });
    }
}

public class SubmitMessageRequest
{
    public string Name { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public IFormFile? Photo { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? SessionId { get; set; }
}

public class RequestViewRequest
{
    public int MessageId { get; set; }
    public string ViewerSessionId { get; set; } = string.Empty;
}

