using FindX.Models;

namespace FindX.Services;

public interface IMessageService
{
    Task<Message> CreateMessageAsync(string name, string messageText, string photoPath, double? latitude, double? longitude, string sessionId);
    Task<List<Message>> GetOtherMessagesAsync(string currentSessionId);
    Task<Message?> GetMessageAsync(int id);
    Task<bool> RequestViewAsync(int messageId, string viewerSessionId);
    Task<bool> CheckMutualConsentAsync(string sessionId1, string sessionId2);
    Task DeleteMessageAsync(int messageId, string sessionId);
}

