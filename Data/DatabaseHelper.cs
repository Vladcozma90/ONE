using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;

public static class DatabaseHelper
{
    private static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ONE", "Data");
    private static readonly string DatabasePath = Path.Combine(AppDataPath, "test1.db");
    private static readonly string ConnectionString = $"Data Source={DatabasePath};Version=3;";

    public static void InitializeDatabase()
    {
        try
        {
            EnsureDatabaseExists();
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            if (!TableExists(connection, "Customer_info") || !TableExists(connection, "User_input_data"))
                throw new Exception("Required tables are missing.");

            MessageBox.Show("Database initialized successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (FileNotFoundException)
        {
            MessageBox.Show($"Database file '{DatabasePath}' is missing.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database initialization failed: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static SQLiteConnection GetConnection()
    {
        EnsureDatabaseExists();

        try
        {
            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            using var command = new SQLiteCommand("PRAGMA journal_mode = WAL;", connection);
            command.ExecuteNonQuery();

            return connection;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database Connection Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private static void EnsureDatabaseExists()
    {
        if (!Directory.Exists(AppDataPath))
            Directory.CreateDirectory(AppDataPath);

        if (!File.Exists(DatabasePath))
            throw new FileNotFoundException($"Database file '{DatabasePath}' not found.");
    }

    private static bool TableExists(SQLiteConnection connection, string tableName)
    {
        using var command = new SQLiteCommand("SELECT 1 FROM sqlite_master WHERE type='table' AND name=@tableName LIMIT 1;", connection);
        command.Parameters.AddWithValue("@tableName", tableName);
        return command.ExecuteScalar() != null;
    }
}
