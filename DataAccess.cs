using Microsoft.Data.Sqlite;
using System.IO;
using System.Windows;
using KumanoKodo.Models;
using Microsoft.Extensions.Configuration;

namespace KumanoKodo;

public interface IDataAccess
{
    void InitializeDatabase();
    SqliteConnection CreateConnection();
    Task<List<QuizQuestion>> GetQuizQuestionsAsync(int userId, int lessonId, int count = 10);
    Task RecordQuizAttemptAsync(int userId, int quizId, bool wasCorrect);
    Task InsertVocabularyAsync(List<VocabularyData> vocabulary);
    Task InsertSentencesAsync(List<SentenceData> sentences);
    Task InsertGrammarTopicsAsync(List<GrammarTopicData> grammarTopics);
    Task<List<GrammarTopicData>> GetGrammarTopicsAsync(int lessonId);
    Task InsertLessonAsync(int lessonId, string? title, string? description);
}

public class DataAccess : IDataAccess
{
    private readonly string _dbPath;
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    public DataAccess(IConfiguration configuration)
    {
        _configuration = configuration;
        string projectRoot = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
        string dbPath = _configuration.GetValue<string>("Database:Path") ?? "database/kumano_kodo.db";
        _dbPath = Path.Combine(projectRoot, dbPath);
        _connectionString = $"Data Source={_dbPath}";
        InitializeDatabase();
    }

    public virtual void InitializeDatabase()
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
                    Title TEXT,
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
                    LessonId INTEGER NOT NULL,
                    Kanji TEXT,
                    Pronunciation TEXT NOT NULL,
                    Romaaji TEXT NOT NULL,
                    Meaning TEXT NOT NULL,
                    FOREIGN KEY(LessonId) REFERENCES Lessons(Id),
                    UNIQUE(LessonId, Pronunciation, Romaaji, Meaning)
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

    public virtual SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    // Quiz-related methods
    public virtual async Task<List<QuizQuestion>> GetQuizQuestionsAsync(int userId, int lessonId, int count = 10)
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
                                WHEN qp.NextReviewDate <= CURRENT_TIMESTAMP THEN 1
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

    public virtual async Task RecordQuizAttemptAsync(int userId, int quizId, bool wasCorrect)
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
                    WHEN @Correct = 1 THEN datetime(@LastAttempted, '+' || (pow(2, ReviewCount)) || ' days')
                    ELSE datetime(@LastAttempted, '+1 hour')
                END";

        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@QuizId", quizId);
        command.Parameters.AddWithValue("@LastAttempted", DateTime.UtcNow);
        command.Parameters.AddWithValue("@Correct", wasCorrect);

        await command.ExecuteNonQueryAsync();
    }

    public virtual async Task InsertVocabularyAsync(List<VocabularyData> vocabulary)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            // First, verify that all lesson IDs exist
            var lessonIds = vocabulary.Select(v => v.LessonId).Distinct();
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
                    SELECT COUNT(*) FROM Lessons WHERE Id IN (" + string.Join(",", lessonIds) + ")";
                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                if (count != lessonIds.Count())
                {
                    throw new InvalidOperationException("Some lesson IDs do not exist in the Lessons table");
                }
            }

            // Insert vocabulary with duplicate handling
            foreach (var item in vocabulary)
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO Vocabulary (LessonId, Kanji, Pronunciation, Romaaji, Meaning)
                    VALUES (@LessonId, @Kanji, @Pronunciation, @Romaaji, @Meaning)
                    ON CONFLICT(LessonId, Pronunciation, Romaaji, Meaning) DO NOTHING";

                command.Parameters.AddWithValue("@LessonId", item.LessonId);
                command.Parameters.AddWithValue("@Kanji", (object?)item.Kanji ?? DBNull.Value);
                command.Parameters.AddWithValue("@Pronunciation", item.Pronunciation);
                command.Parameters.AddWithValue("@Romaaji", item.Romaaji);
                command.Parameters.AddWithValue("@Meaning", item.Meaning);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task InsertSentencesAsync(List<SentenceData> sentences)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var item in sentences)
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO Sentences (LessonId, Kanji, Pronunciation, Romaaji, Translation)
                    VALUES (@LessonId, @Kanji, @Pronunciation, @Romaaji, @Translation)";

                command.Parameters.AddWithValue("@LessonId", item.LessonId);
                command.Parameters.AddWithValue("@Kanji", item.Kanji);
                command.Parameters.AddWithValue("@Pronunciation", item.Pronunciation);
                command.Parameters.AddWithValue("@Romaaji", item.Romaaji);
                command.Parameters.AddWithValue("@Translation", item.Translation);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task InsertGrammarTopicsAsync(List<GrammarTopicData> grammarTopics)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var item in grammarTopics)
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO GrammarTopics (LessonId, Title, PDF_Link)
                    VALUES (@LessonId, @Title, @PDF_Link)";

                command.Parameters.AddWithValue("@LessonId", item.LessonId);
                command.Parameters.AddWithValue("@Title", item.Title);
                command.Parameters.AddWithValue("@PDF_Link", item.PDF_Link);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public virtual async Task<List<GrammarTopicData>> GetGrammarTopicsAsync(int lessonId)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT GrammarId, LessonId, Title, PDF_Link
            FROM GrammarTopics
            WHERE LessonId = @LessonId
            ORDER BY Title";

        command.Parameters.AddWithValue("@LessonId", lessonId);

        var topics = new List<GrammarTopicData>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            topics.Add(new GrammarTopicData
            {
                Title = reader.GetString(reader.GetOrdinal("Title")),
                LessonId = reader.GetInt32(reader.GetOrdinal("LessonId")),
                PDF_Link = reader.GetString(reader.GetOrdinal("PDF_Link"))
            });
        }

        return topics;
    }

    public virtual async Task InsertLessonAsync(int lessonId, string? title, string? description)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Lessons (Id, Title, Description)
            VALUES (@Id, @Title, @Description)
            ON CONFLICT(Id) DO NOTHING";

        command.Parameters.AddWithValue("@Id", lessonId);
        command.Parameters.AddWithValue("@Title", (object?)title ?? DBNull.Value);
        command.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);

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
