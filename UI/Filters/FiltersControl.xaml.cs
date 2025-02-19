using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ONE.UI.Filters
{
    public partial class FiltersControl : UserControl
    {
        private static string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ONE", "Data", "test1.db");
        private static string connectionString = $"Data Source={databasePath};Version=3;";

        public event Action<string, string> FiltersUpdated;

        public FiltersControl()
        {
            InitializeComponent();
            this.Loaded += FiltersControl_Loaded;
        }

        private void FiltersControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DatabaseAndTableExist())
                {
                    LoadFilterData();
                }
                else
                {
                    MessageBox.Show("Database or 'Customer_info' table not found!",
                                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing FiltersControl: {ex.Message}",
                                "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool DatabaseAndTableExist()
        {
            if (!File.Exists(databasePath))
                return false;

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT name FROM sqlite_master WHERE type='table' AND name='Customer_info';";
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private void LoadFilterData()
        {
            LoadFilterOptions("Region", RegionDropdown);
            LoadFilterOptions("Cluster", ClusterDropdown);
            LoadFilterOptions("Team", TeamDropdown);
            LoadFilterOptions("ESA", ESADropdown);
            LoadFilterOptions("Customer_name", CustomerDropdown); // ✅ Updated column name
        }

        private void LoadFilterOptions(string columnName, ComboBox dropdown)
        {
            List<string> options = new List<string>();

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // ✅ Ensure correct column names
                    string query = $"SELECT DISTINCT {columnName} FROM Customer_info WHERE {columnName} IS NOT NULL ORDER BY {columnName}";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            options.Add(reader[columnName]?.ToString() ?? "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading {columnName}: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            dropdown.ItemsSource = options;
        }

        private void FilterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox dropdown && dropdown.SelectedItem != null)
            {
                string filterType = dropdown.Name.Replace("Dropdown", "");
                string selectedValue = dropdown.SelectedItem.ToString();
                FiltersUpdated?.Invoke(filterType, selectedValue);
            }
        }
    }
}
