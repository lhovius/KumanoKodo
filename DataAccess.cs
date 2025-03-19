using Microsoft.Data.Sqlite;
using System.IO;

namespace KumanoKodo;

public class DataAccess
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public DataAccess()
    {
        _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database", "kumano_kodo.db");
        _connectionString = $"Data Source={_dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        // Ensure the database directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);

        // Create database and tables if they don't exist
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        CreateTables(connection);
    }

    private void CreateTables(SqliteConnection connection)
    {
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
                    LessonId INTEGER NOT NULL,
                    Completed BOOLEAN NOT NULL DEFAULT 0,
                    FOREIGN KEY(LessonId) REFERENCES Lessons(Id)
                )";
            command.ExecuteNonQuery();
        }
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
} 
