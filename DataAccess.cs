using Microsoft.Data.Sqlite;
using System.IO;
using System.Windows;

namespace KumanoKodo;

public class DataAccess
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public DataAccess()
    {
        string projectRoot = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
        _dbPath = Path.Combine(projectRoot, "database", "kumano_kodo.db");
        _connectionString = $"Data Source={_dbPath}";
        InitializeDatabase();
    }

    public void InitializeDatabase()
    {
        // Ensure the database directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
        //MessageBox.Show("Database Path: " + _connectionString);

        // Create database and tables if they don't exist
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        CreateTables(connection);
    }

    private void CreateTables(SqliteConnection connection)
    {
        // Create Users table
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY,
                    Username TEXT NOT NULL UNIQUE,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )";
            command.ExecuteNonQuery();
        }

        // Create Lessons table
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Lessons (
                    Id INTEGER PRIMARY KEY,
                    Title TEXT NOT NULL,
                    Description TEXT
                )";
            command.ExecuteNonQuery();
        }

        // Create Vocabulary table
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Vocabulary (
                    Id INTEGER PRIMARY KEY,
                    Word TEXT NOT NULL,
                    Meaning TEXT NOT NULL,
                    LessonId INTEGER,
                    FOREIGN KEY(LessonId) REFERENCES Lessons(Id)
                )";
            command.ExecuteNonQuery();
        }

        // Create UserProgress table
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserProgress (
                    Id INTEGER PRIMARY KEY,
                    UserId INTEGER NOT NULL,
                    LessonId INTEGER NOT NULL,
                    Completed BOOLEAN NOT NULL DEFAULT 0,
                    FOREIGN KEY(UserId) REFERENCES Users(Id),
                    FOREIGN KEY(LessonId) REFERENCES Lessons(Id)
                )";
            command.ExecuteNonQuery();
        }

        // Create Quizzes table
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Quizzes (
                    Id INTEGER PRIMARY KEY,
                    LessonId INTEGER NOT NULL,
                    Question TEXT NOT NULL,
                    Answer1 TEXT NOT NULL,
                    Answer2 TEXT NOT NULL,
                    Answer3 TEXT NOT NULL,
                    Answer4 TEXT NOT NULL,
                    CorrectAnswer INTEGER NOT NULL,
                    LastReviewed DATETIME,
                    ReviewCount INTEGER DEFAULT 0,
                    Difficulty INTEGER DEFAULT 0,
                    FOREIGN KEY(LessonId) REFERENCES Lessons(Id)
                )";
            command.ExecuteNonQuery();
        }

        // Create QuizProgress table for spaced repetition
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS QuizProgress (
                    Id INTEGER PRIMARY KEY,
                    UserId INTEGER NOT NULL,
                    QuizId INTEGER NOT NULL,
                    LastAttempted DATETIME,
                    Correct BOOLEAN,
                    IncorrectAttempts INTEGER DEFAULT 0,
                    NextReviewDate DATETIME,
                    FOREIGN KEY(UserId) REFERENCES Users(Id),
                    FOREIGN KEY(QuizId) REFERENCES Quizzes(Id)
                )";
            command.ExecuteNonQuery();
        }
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    // Quiz-related methods
    public async Task<List<QuizQuestion>> GetQuizQuestionsAsync(int userId, int lessonId, int count = 10)
    {
        var questions = new List<QuizQuestion>();
        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            WITH RankedQuizzes AS (
                SELECT 
                    q.*,
                    qp.LastAttempted,
                    qp.Correct,
                    qp.IncorrectAttempts,
                    qp.NextReviewDate,
                    ROW_NUMBER() OVER (
                        ORDER BY 
                            CASE 
                                WHEN qp.LastAttempted IS NULL THEN 0
                                WHEN qp.NextReviewDate > CURRENT_TIMESTAMP THEN 1
                                WHEN qp.Correct = 1 THEN 2
                                ELSE 3
                            END,
                            qp.IncorrectAttempts DESC,
                            RANDOM()
                    ) as RowNum
                FROM Quizzes q
                LEFT JOIN QuizProgress qp ON q.Id = qp.QuizId AND qp.UserId = @UserId
                WHERE q.LessonId = @LessonId
            )
            SELECT * FROM RankedQuizzes WHERE RowNum <= @Count";

        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@LessonId", lessonId);
        command.Parameters.AddWithValue("@Count", count);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            questions.Add(new QuizQuestion
            {
                Id = reader.GetInt32(0),
                LessonId = reader.GetInt32(1),
                Question = reader.GetString(2),
                Answers = new[]
                {
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetString(6)
                },
                CorrectAnswer = reader.GetInt32(7),
                LastReviewed = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                WasCorrect = reader.IsDBNull(9) ? null : reader.GetBoolean(9),
                ReviewCount = reader.GetInt32(10),
                Difficulty = reader.GetInt32(11),
                IncorrectAttempts = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
                NextReviewDate = reader.IsDBNull(13) ? null : reader.GetDateTime(13)
            });
        }

        return questions;
    }

    public async Task RecordQuizAttemptAsync(int userId, int quizId, bool wasCorrect)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO QuizProgress (
                UserId, QuizId, LastAttempted, Correct, IncorrectAttempts, NextReviewDate
            )
            VALUES (
                @UserId, @QuizId, @LastAttempted, @Correct,
                CASE WHEN @Correct = 0 THEN 1 ELSE 0 END,
                CASE 
                    WHEN @Correct = 1 THEN datetime(@LastAttempted, '+1 day')
                    ELSE datetime(@LastAttempted, '+1 hour')
                END
            )
            ON CONFLICT(UserId, QuizId) DO UPDATE SET
                LastAttempted = @LastAttempted,
                Correct = @Correct,
                IncorrectAttempts = IncorrectAttempts + CASE WHEN @Correct = 0 THEN 1 ELSE 0 END,
                NextReviewDate = CASE 
                    WHEN @Correct = 1 THEN datetime(@LastAttempted, '+' || (IncorrectAttempts + 1) || ' days')
                    ELSE datetime(@LastAttempted, '+1 hour')
                END";

        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@QuizId", quizId);
        command.Parameters.AddWithValue("@LastAttempted", DateTime.UtcNow);
        command.Parameters.AddWithValue("@Correct", wasCorrect);

        await command.ExecuteNonQueryAsync();
    }
}

public class QuizQuestion
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string[] Answers { get; set; } = Array.Empty<string>();
    public int CorrectAnswer { get; set; }
    public DateTime? LastReviewed { get; set; }
    public bool? WasCorrect { get; set; }
    public int ReviewCount { get; set; }
    public int Difficulty { get; set; }
    public int IncorrectAttempts { get; set; }
    public DateTime? NextReviewDate { get; set; }
} 
