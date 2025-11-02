using Microsoft.EntityFrameworkCore;
using FindX.Data;
using FindX.Models;

namespace FindX.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public MessageService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<Message> CreateMessageAsync(string name, string messageText, string photoPath, double? latitude, double? longitude, string sessionId)
    {
        var message = new Message
        {
            Name = name,
            MessageText = messageText,
            PhotoPath = photoPath,
            Latitude = latitude,
            Longitude = longitude,
            SessionId = sessionId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<List<Message>> GetOtherMessagesAsync(string currentSessionId)
    {
        return await _context.Messages
            .Where(m => m.SessionId != currentSessionId)
            .Include(m => m.Consents)
            .ToListAsync();
    }

    public async Task<Message?> GetMessageAsync(int id)
    {
        return await _context.Messages
            .Include(m => m.Consents)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> RequestViewAsync(int messageId, string viewerSessionId)
    {
        var message = await GetMessageAsync(messageId);
        if (message == null) return false;

        // Check if consent already exists
        var existingConsent = await _context.MessageConsents
            .FirstOrDefaultAsync(c => c.MessageId == messageId && c.ViewerSessionId == viewerSessionId);

        if (existingConsent != null) return false;

        // Create new consent
        var consent = new MessageConsent
        {
            MessageId = messageId,
            ViewerSessionId = viewerSessionId,
            ViewedAt = DateTime.UtcNow
        };

        _context.MessageConsents.Add(consent);

        // Check if there's mutual consent (if the message owner has viewed this viewer's message)
        var viewerMessage = await _context.Messages
            .Include(m => m.Consents)
            .FirstOrDefaultAsync(m => m.SessionId == viewerSessionId);

        if (viewerMessage != null)
        {
            var reverseConsent = viewerMessage.Consents
                .FirstOrDefault(c => c.ViewerSessionId == message.SessionId);

            if (reverseConsent != null)
            {
                // Mutual consent exists - mark both as having mutual consent
                consent.HasMutualConsent = true;
                reverseConsent.HasMutualConsent = true;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CheckMutualConsentAsync(string sessionId1, string sessionId2)
    {
        var message1 = await _context.Messages
            .Include(m => m.Consents)
            .FirstOrDefaultAsync(m => m.SessionId == sessionId1);

        var message2 = await _context.Messages
            .Include(m => m.Consents)
            .FirstOrDefaultAsync(m => m.SessionId == sessionId2);

        if (message1 == null || message2 == null) return false;

        var consent1 = message1.Consents.FirstOrDefault(c => c.ViewerSessionId == sessionId2);
        var consent2 = message2.Consents.FirstOrDefault(c => c.ViewerSessionId == sessionId1);

        return consent1 != null && consent2 != null && consent1.HasMutualConsent && consent2.HasMutualConsent;
    }

    public async Task DeleteMessageAsync(int messageId, string sessionId)
    {
        var message = await GetMessageAsync(messageId);
        if (message != null && message.SessionId == sessionId)
        {
            // Delete associated photo
            if (!string.IsNullOrEmpty(message.PhotoPath))
            {
                var photoPath = Path.Combine(_environment.WebRootPath, message.PhotoPath.TrimStart('/'));
                if (File.Exists(photoPath))
                {
                    File.Delete(photoPath);
                }
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }
    }
}

