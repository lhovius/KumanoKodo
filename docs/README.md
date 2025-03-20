# Kumano Kodo Japanese Learning Application

A WPF application for learning Japanese through the context of the Kumano Kodo pilgrimage routes.

## Project Structure

```
KumanoKodo/
├── database/               # SQLite database files
├── docs/                  # Documentation
├── Views/                 # WPF pages and views
├── ViewModels/            # MVVM view models
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

### Vocabulary
- `Id` (INTEGER PRIMARY KEY)
- `Word` (TEXT NOT NULL)
- `Meaning` (TEXT NOT NULL)
- `LessonId` (INTEGER, FOREIGN KEY)

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

The quiz system implements a spaced repetition algorithm:

1. Question Priority:
   - Never attempted questions
   - Questions due for review
   - Correctly answered questions
   - Incorrectly answered questions

2. Review Intervals:
   - Correct answers: Increasing intervals (1 day, 2 days, etc.)
   - Incorrect answers: 1-hour interval
   - Based on incorrect attempt count

3. Progress Tracking:
   - Tracks incorrect attempts
   - Shows next review date
   - Displays attempt history

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio
3. Build and run the application
4. The database will be automatically initialized in the `database` folder

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details. 