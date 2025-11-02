namespace FindX.Models;

public class Message
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string SessionId { get; set; } = string.Empty; // Unique identifier for the user session
    
    // Mutual consent tracking
    public List<MessageConsent> Consents { get; set; } = new();
}

public class MessageConsent
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public Message Message { get; set; } = null!;
    public string ViewerSessionId { get; set; } = string.Empty; // Session ID of the person viewing
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    public bool HasMutualConsent { get; set; } = false; // True when both parties have viewed each other's messages
}

