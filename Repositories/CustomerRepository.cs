using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;

public static class CustomerRepository
{
    public static void DebugDatabase()
    {
        using (var connection = DatabaseHelper.GetConnection())
        {
            try
            {
                connection.Open();
                MessageBox.Show("Database connection opened successfully!", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);

                // ✅ Check if the table exists
                string checkTableQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='Customer_info'";
                using (var cmd = new SQLiteCommand(checkTableQuery, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        MessageBox.Show("Table `Customer_info` does NOT exist!", "Database Debug", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // ✅ Count the rows
                string countQuery = "SELECT COUNT(*) FROM Customer_info";
                using (var countCmd = new SQLiteCommand(countQuery, connection))
                {
                    int rowCount = Convert.ToInt32(countCmd.ExecuteScalar());
                    MessageBox.Show($"Customer_info table contains {rowCount} rows.", "Database Debug", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (rowCount == 0)
                    {
                        MessageBox.Show("Table exists but contains 0 rows!", "Database Debug", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // ✅ Fetch first 5 rows (column names explicitly listed)
                string query = "SELECT Id, Customer_name, Incoterm, Bill_to, `d-code`, Destination, Ship_to, Freight_cost, Country, Customer_contact, Specific_notes FROM Customer_info LIMIT 5";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    string debugOutput = "Retrieved Rows:\n";
                    int readCount = 0;

                    while (reader.Read())
                    {
                        try
                        {
                            readCount++;

                            // ✅ Explicitly checking values for debugging
                            int id = reader.GetInt32(0);
                            string name = reader.IsDBNull(1) ? "NULL" : reader.GetString(1);
                            string incoterm = reader.IsDBNull(2) ? "NULL" : reader.GetString(2);
                            int billTo = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                            int dCode = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                            string destination = reader.IsDBNull(5) ? "NULL" : reader.GetString(5);
                            int shipTo = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                            string freightCost = reader.IsDBNull(7) ? "NULL" : reader.GetString(7);
                            string country = reader.IsDBNull(8) ? "NULL" : reader.GetString(8); // REAL -> String fix
                            string contact = reader.IsDBNull(9) ? "NULL" : reader.GetString(9);
                            string notes = reader.IsDBNull(10) ? "NULL" : reader.GetString(10);

                            debugOutput += $"ID: {id}, Name: {name}, Contact: {contact}, Country: {country}\n";
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error reading row {readCount}: {ex.Message}", "Data Read Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    if (readCount == 0)
                    {
                        MessageBox.Show("Query executed but returned 0 rows!", "Database Debug", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show(debugOutput, "Database Debug", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database Debug Error: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
