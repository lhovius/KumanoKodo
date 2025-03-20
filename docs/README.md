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