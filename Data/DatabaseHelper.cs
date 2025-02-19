using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;

public static class DatabaseHelper
{
    private static string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ONE", "Data");
    private static string databasePath = Path.Combine(appDataPath, "test1.db");
    private static string connectionString = $"Data Source={databasePath};Version=3;";

    public static void InitializeDatabase()
    {
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        MessageBox.Show($"DatabaseHelper is using database path: {databasePath}");

        if (!File.Exists(databasePath))
        {
            MessageBox.Show($"Error: The database file '{databasePath}' is missing.",
                            "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new FileNotFoundException($"Database file '{databasePath}' not found.");
        }

        try
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                if (!TableExists(connection, "Customer_info") || !TableExists(connection, "Shipments"))
                {
                    MessageBox.Show("Error: The database structure is incomplete. Required tables are missing.",
                                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new Exception("Required tables are missing.");
                }

                MessageBox.Show("Database initialized successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database initialization failed: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static SQLiteConnection GetConnection()
    {
        try
        {
            if (!File.Exists(databasePath))
            {
                throw new FileNotFoundException($"Database file '{databasePath}' not found.");
            }

            var connection = new SQLiteConnection(connectionString);
            connection.Open();

            // Check if connection is actually open
            if (connection.State != System.Data.ConnectionState.Open)
            {
                throw new Exception("Database connection failed.");
            }

            // Prevent database locking issues
            using (var command = new SQLiteCommand("PRAGMA journal_mode = WAL;", connection))
            {
                command.ExecuteNonQuery();
            }

            return connection;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database Connection Error: {ex.Message}\n\n{ex.StackTrace}",
                            "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private static bool TableExists(SQLiteConnection connection, string tableName)
    {
        string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";

        using (var command = new SQLiteCommand(query, connection))
        {
            command.Parameters.AddWithValue("@tableName", tableName);
            using (var reader = command.ExecuteReader())
            {
                return reader.HasRows;
            }
        }
    }
}
