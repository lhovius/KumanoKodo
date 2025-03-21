# Database Schema

## Entity Relationship Diagram

```mermaid
erDiagram
    Users ||--o{ UserProgress : has
    Users ||--o{ QuizProgress : has
    Lessons ||--o{ Vocabulary : contains
    Lessons ||--o{ Quizzes : contains
    Lessons ||--o{ UserProgress : tracks
    Quizzes ||--o{ QuizProgress : tracks

    Users {
        int Id PK
        string Username
        datetime CreatedAt
    }

    Lessons {
        int Id PK
        string Title
        string Description
    }

    Vocabulary {
        int Id PK
        string Word
        string Meaning
        int LessonId FK
    }

    UserProgress {
        int Id PK
        int UserId FK
        int LessonId FK
        bool Completed
    }

    Quizzes {
        int Id PK
        int LessonId FK
        string Question
        string Answer1
        string Answer2
        string Answer3
        string Answer4
        int CorrectAnswer
        datetime LastReviewed
        int ReviewCount
        int Difficulty
    }

    QuizProgress {
        int Id PK
        int UserId FK
        int QuizId FK
        datetime LastAttempted
        bool Correct
        int IncorrectAttempts
        datetime NextReviewDate
    }
```

## Table Descriptions

### Users
Stores user information for multi-user support.

### Lessons
Contains the main learning content organized into lessons.

### Vocabulary
Stores Japanese words and their meanings, linked to specific lessons.

### UserProgress
Tracks user completion status for each lesson.

### Quizzes
Contains quiz questions and answers for each lesson.

### QuizProgress
Implements spaced repetition by tracking:
- User attempts
- Correct/incorrect answers
- Review intervals
- Next review dates

## Relationships

1. Users to UserProgress: One-to-many
   - Each user can have multiple lesson progress records

2. Users to QuizProgress: One-to-many
   - Each user can have multiple quiz attempt records

3. Lessons to Vocabulary: One-to-many
   - Each lesson contains multiple vocabulary words

4. Lessons to Quizzes: One-to-many
   - Each lesson contains multiple quiz questions

5. Lessons to UserProgress: One-to-many
   - Each lesson can have multiple user progress records

6. Quizzes to QuizProgress: One-to-many
   - Each quiz can have multiple attempt records

## Spaced Repetition Implementation

The quiz system uses a sophisticated spaced repetition algorithm implemented in the database:

### Question Selection Logic
```sql
CASE 
    WHEN qp.LastAttempted IS NULL THEN 0
    WHEN qp.NextReviewDate <= CURRENT_TIMESTAMP THEN 1
    WHEN qp.Correct = 1 THEN 2
    ELSE 3
END
```

### Review Intervals
- Correct answers: Exponential growth (2^n days)
  - First correct: 1 day
  - Second correct: 2 days
  - Third correct: 4 days
  - Fourth correct: 8 days
  - And so on...
- Incorrect answers: 1-hour interval

### Progress Tracking
- `LastAttempted`: Records when the question was last answered
- `Correct`: Boolean indicating if the last attempt was correct
- `IncorrectAttempts`: Counter for failed attempts
- `NextReviewDate`: Calculated based on performance
  - For correct answers: LastAttempted + (2^ReviewCount) days
  - For incorrect answers: LastAttempted + 1 hour 

## Tables

### Lessons
- `LessonId` (PK) - INTEGER PRIMARY KEY
- `Title` - TEXT
- `Description` - TEXT
- `Order` - INTEGER
- `ImageUrl` - TEXT
- `AudioUrl` - TEXT
- `IsCompleted` - INTEGER (0/1)

### Vocabulary
- `VocabularyId` (PK) - INTEGER PRIMARY KEY
- `LessonId` (FK) - INTEGER
- `Kanji` - TEXT
- `Pronunciation` - TEXT (Hiragana/Katakana)
- `Romaaji` - TEXT
- `Meaning` - TEXT
- `ImageUrl` - TEXT
- `AudioUrl` - TEXT
- FOREIGN KEY (`LessonId`) REFERENCES `Lessons`(`LessonId`)

### Sentences
- `SentenceId` (PK) - INTEGER PRIMARY KEY
- `LessonId` (FK) - INTEGER
- `Kanji` - TEXT
- `Pronunciation` - TEXT (Hiragana/Katakana)
- `Romaaji` - TEXT
- `Translation` - TEXT
- `AudioUrl` - TEXT
- FOREIGN KEY (`LessonId`) REFERENCES `Lessons`(`LessonId`)

### GrammarTopics
- `GrammarId` (PK) - INTEGER PRIMARY KEY
- `LessonId` (FK) - INTEGER
- `Title` - TEXT
- `PDF_Link` - TEXT
- FOREIGN KEY (`LessonId`) REFERENCES `Lessons`(`LessonId`)

### Quizzes
- `QuizId` (PK) - INTEGER PRIMARY KEY
- `LessonId` (FK) - INTEGER
- `Question` - TEXT
- `CorrectAnswer` - TEXT
- `Options` - TEXT (JSON array)
- `ImageUrl` - TEXT
- `AudioUrl` - TEXT
- FOREIGN KEY (`LessonId`) REFERENCES `Lessons`(`LessonId`)

### UserProgress
- `ProgressId` (PK) - INTEGER PRIMARY KEY
- `UserId` (FK) - INTEGER
- `LessonId` (FK) - INTEGER
- `CompletedDate` - TEXT
- `ProgressImageUrl` - TEXT
- FOREIGN KEY (`UserId`) REFERENCES `Users`(`UserId`)
- FOREIGN KEY (`LessonId`) REFERENCES `Lessons`(`LessonId`)

### QuizAttempts
- `AttemptId` (PK) - INTEGER PRIMARY KEY
- `UserId` (FK) - INTEGER
- `QuizId` (FK) - INTEGER
- `IsCorrect` - INTEGER (0/1)
- `AttemptDate` - TEXT
- `NextReviewDate` - TEXT
- FOREIGN KEY (`UserId`) REFERENCES `Users`(`UserId`)
- FOREIGN KEY (`QuizId`) REFERENCES `Quizzes`(`QuizId`)

### Users
- `UserId` (PK) - INTEGER PRIMARY KEY
- `Username` - TEXT UNIQUE
- `PasswordHash` - TEXT
- `Email` - TEXT UNIQUE
- `CreatedDate` - TEXT

## Data Sources and Licensing

### Marugoto Materials
The application uses vocabulary and sentences from the Marugoto Japanese Language and Culture Starter A1 course materials:
- Vocabulary: MarugotoStarterActivitiesVocabularyIndex2_EN.pdf
- Sentences: MarugotoStarterActivitiesPhraseIndex.pdf
- Grammar: MarugotoStarterActivitiesGrammarIndex.pdf

**Important Note**: These materials must be downloaded manually by users. The application does not include or distribute the PDF files directly.

### Media Storage
All media files (images, audio) are stored in Azure Blob Storage with the following structure:
- Lessons: `lessons/{lessonId}.{extension}`
- Vocabulary: `vocabulary/{vocabularyId}.{extension}`
- Sentences: `sentences/{sentenceId}.{extension}`
- Progress: `progress/{userId}/{lessonId}.{extension}`
- Quiz: `quiz/{quizId}.{extension}` 