# Kumano Kodo Japanese Learning Application

A WPF application for learning Japanese through the context of the Kumano Kodo pilgrimage routes.

## Project Structure

```
KumanoKodo/
├── database/               # SQLite database files
├── docs/                  # Documentation
├── Views/                 # WPF pages and views
│   ├── HomePage.xaml      # Welcome and introduction
│   ├── LessonsPage.xaml   # Lesson selection and content
│   ├── QuizPage.xaml      # Interactive quiz interface
│   └── ProgressPage.xaml  # Learning progress tracking
├── ViewModels/            # MVVM view models
├── Models/                # Data models
├── Services/              # External services
│   └── AzureBlobService.cs # Azure Blob Storage integration
├── Converters/            # WPF value converters
└── DataAccess.cs          # Database access layer
```

## Database Schema

The application uses SQLite with the following tables:

### Users
- `Id` (INTEGER PRIMARY KEY)
- `Username` (TEXT NOT NULL UNIQUE)
- `CreatedAt` (DATETIME)

### Lessons
- `Id` (INTEGER PRIMARY KEY)
- `Title` (TEXT NOT NULL)
- `Description` (TEXT)
- `ImageUrl` (TEXT) - URL to lesson image in Azure Blob Storage
- `AudioUrl` (TEXT) - URL to lesson audio in Azure Blob Storage

### Vocabulary
- `Id` (INTEGER PRIMARY KEY)
- `Word` (TEXT NOT NULL)
- `Meaning` (TEXT NOT NULL)
- `LessonId` (INTEGER, FOREIGN KEY)
- `ImageUrl` (TEXT) - URL to vocabulary image in Azure Blob Storage
- `AudioUrl` (TEXT) - URL to vocabulary audio in Azure Blob Storage

### UserProgress
- `Id` (INTEGER PRIMARY KEY)
- `UserId` (INTEGER, FOREIGN KEY)
- `LessonId` (INTEGER, FOREIGN KEY)
- `Completed` (BOOLEAN)

### Quizzes
- `Id` (INTEGER PRIMARY KEY)
- `LessonId` (INTEGER, FOREIGN KEY)
- `Question` (TEXT NOT NULL)
- `Answer1-4` (TEXT NOT NULL)
- `CorrectAnswer` (INTEGER)
- `LastReviewed` (DATETIME)
- `ReviewCount` (INTEGER)
- `Difficulty` (INTEGER)

### QuizProgress
- `Id` (INTEGER PRIMARY KEY)
- `UserId` (INTEGER, FOREIGN KEY)
- `QuizId` (INTEGER, FOREIGN KEY)
- `LastAttempted` (DATETIME)
- `Correct` (BOOLEAN)
- `IncorrectAttempts` (INTEGER)
- `NextReviewDate` (DATETIME)

## Azure Storage Configuration

The application uses Azure Blob Storage with SAS token authentication for secure media storage.

### Development Setup

1. Create an `appsettings.json` file in the project root:
   ```json
   {
     "AzureStorage": {
       "BlobServiceUrl": "https://your-storage-account.blob.core.windows.net",
       "SasToken": "your-sas-token-here"
     }
   }
   ```

2. Copy `appsettings.example.json` to `appsettings.json` and update with your values.

### Production Setup

In production, use environment variables:
```powershell
$env:AZURE_STORAGE_BLOB_SERVICE_URL="https://your-storage-account.blob.core.windows.net"
$env:AZURE_STORAGE_SAS_TOKEN="your-sas-token-here"
```

### Generating SAS Tokens

1. In Azure Portal:
   - Navigate to your storage account
   - Select "Shared access signature"
   - Configure permissions:
     - Read, Write, List
     - Container-level access
     - Set appropriate expiry time
   - Generate SAS token

2. Security Best Practices:
   - Use container-level SAS tokens
   - Set appropriate expiry times
   - Limit permissions to minimum required
   - Rotate tokens regularly
   - Never commit tokens to source control

### File Organization

The application uses the following structure in Azure Blob Storage:

1. Container: `kumano-assets`
   - Stores images and audio files
   - Organized by lesson and vocabulary items

2. File Paths:
   - Lesson media: `lessons/{lessonId}/`
     - `image.jpg`: Lesson illustration
     - `audio.mp3`: Lesson audio content
   - Vocabulary media: `vocabulary/{wordId}/`
     - `image.jpg`: Word illustration
     - `audio.mp3`: Word pronunciation

3. Features:
   - Secure file storage
   - CDN integration support
   - Efficient media delivery
   - Reduced local storage usage

4. Configuration:
   - SAS token authentication
   - Container created automatically
   - Content types set automatically

## UI Navigation

The application follows a hierarchical navigation structure:

1. Main Window
   - Home Page
   - Lessons Page
   - Quiz Page
   - Progress Page

2. Quiz System
   - Question Display
   - Answer Selection
   - Progress Tracking
   - Spaced Repetition

## Spaced Repetition System

The quiz system implements a sophisticated spaced repetition algorithm:

1. Question Priority:
   - Never attempted questions (priority 0)
   - Questions due for review (NextReviewDate <= CURRENT_TIMESTAMP)
   - Correctly answered questions
   - Incorrectly answered questions

2. Review Intervals:
   - Correct answers: Exponential growth (2^n days)
     - First correct: 1 day
     - Second correct: 2 days
     - Third correct: 4 days
     - Fourth correct: 8 days
     - And so on...
   - Incorrect answers: 1-hour interval
   - Based on incorrect attempt count

3. Progress Tracking:
   - Tracks incorrect attempts
   - Shows next review date
   - Displays attempt history
   - Updates review intervals dynamically

4. Question Selection Logic:
   ```sql
   CASE 
       WHEN qp.LastAttempted IS NULL THEN 0
       WHEN qp.NextReviewDate <= CURRENT_TIMESTAMP THEN 1
       WHEN qp.Correct = 1 THEN 2
       ELSE 3
   END
   ```
   This ensures:
   - New questions appear first
   - Due questions are prioritized
   - Correct questions are delayed
   - Incorrect questions are reviewed more frequently

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio
3. Install required NuGet packages:
   - Azure.Storage.Blobs (12.24.0)
4. Configure Azure Blob Storage:
   - Add connection string to app settings
   - Create container named 'kumano-assets'
5. Build and run the application
6. The database will be automatically initialized in the `database` folder

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details. 