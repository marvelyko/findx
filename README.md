# FindX

A 2-page .NET web application that allows users to send messages with photos and location, with a mutual consent system for message visibility.

## Features

### Page 1: Send Message
- Upload your photo
- Enter your name
- Enter your message
- Grant location permission (required)
- Send button to submit data to backend

### Page 2: View Messages
- View messages from other users
- Messages are hidden by default
- Messages only become visible when two users mutually consent to view each other's messages
- Finish button to delete your message and return to the main page

## Prerequisites

- .NET 8.0 SDK or later
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or VS Code (optional)

## Getting Started

1. **Restore packages and build the project:**
   ```bash
   dotnet restore
   dotnet build
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Open your browser:**
   Navigate to `https://localhost:5001` or `http://localhost:5000` (check the console output for the exact URL)

## Database

The application uses Entity Framework Core with SQL Server LocalDB by default. The database will be automatically created on first run.

To use a different database, update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your connection string here"
  }
}
```

## How It Works

1. **Session Management**: Each user gets a unique session ID stored in browser localStorage
2. **Message Submission**: Users submit messages with photo, name, message text, and location
3. **Mutual Consent**: Messages are hidden until two users both request to view each other's messages
4. **Message Deletion**: Users can finish and delete their own message, which removes it from the database

## Project Structure

- `Pages/` - Razor pages (Index.cshtml and ViewMessages.cshtml)
- `Controllers/` - API controllers for message operations
- `Models/` - Data models (Message, MessageConsent)
- `Services/` - Business logic services
- `Data/` - Entity Framework database context
- `wwwroot/uploads/` - Uploaded photos storage

## Notes

- Photos are stored in `wwwroot/uploads/` directory
- The mutual consent system ensures privacy - messages only appear when both parties agree
- Location permission is required for message submission
