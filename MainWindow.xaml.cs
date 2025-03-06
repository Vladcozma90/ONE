using ONE.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;  // For VisualTreeHelper if needed

namespace ONE
{
    public partial class MainWindow : Window
    {
        private bool isUpdatingFilters = false; // Prevents recursion
        // Master dictionary loaded from the database (remains unchanged during the session)
        private Dictionary<string, List<string>> originalFilterValues;
        // Current dictionary of unique values currently displayed (used for search)
        private Dictionary<string, List<string>> currentFilterValues;

        public MainWindow()
        {
            InitializeComponent();
            LoadFilters();
        }

        private void LoadFilters()
        {
            // Load the full dataset from the repository.
            originalFilterValues = CustomerRepository.GetFilteredUniqueValues("", "", "", "", "");
            // Clone the master list into current list.
            currentFilterValues = originalFilterValues.ToDictionary(kvp => kvp.Key, kvp => new List<string>(kvp.Value));
            PopulateAllDropdowns(currentFilterValues);
        }

        private void PopulateAllDropdowns(Dictionary<string, List<string>> filterValues)
        {
            PopulateDropdown(RegionDropdown, filterValues, "region");
            PopulateDropdown(ClusterDropdown, filterValues, "cluster");
            PopulateDropdown(TeamDropdown, filterValues, "team");
            PopulateDropdown(EsaDropdown, filterValues, "esa");
            // Use "customer" as the key since that's what the repository returns.
            PopulateDropdown(CustomerDropdown, filterValues, "customer");
        }

        private void PopulateDropdown(ComboBox dropdown, Dictionary<string, List<string>> filterValues, string key)
        {
            if (!filterValues.ContainsKey(key))
                return;

            dropdown.SelectionChanged -= FilterSelectionChanged;
            dropdown.Items.Clear();
            dropdown.Items.Add("All");
            foreach (var value in filterValues[key])
            {
                dropdown.Items.Add(value);
            }
            dropdown.SelectedIndex = 0;
            dropdown.SelectionChanged += FilterSelectionChanged;
        }

        private void FilterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isUpdatingFilters) return;
            ApplyCurrentFilters();
        }

        private void ApplyCurrentFilters()
        {
            isUpdatingFilters = true;
            try
            {
                string selectedRegion = RegionDropdown.SelectedItem?.ToString() ?? "All";
                string selectedCluster = ClusterDropdown.SelectedItem?.ToString() ?? "All";
                string selectedTeam = TeamDropdown.SelectedItem?.ToString() ?? "All";
                string selectedEsa = EsaDropdown.SelectedItem?.ToString() ?? "All";
                string selectedCustomer = CustomerDropdown.SelectedItem?.ToString() ?? "All";

                bool isResetting = selectedRegion == "All" && selectedCluster == "All" &&
                                   selectedTeam == "All" && selectedEsa == "All" &&
                                   selectedCustomer == "All";

                Dictionary<string, List<string>> filteredValues;
                if (isResetting)
                {
                    // Reset current list to the original master list.
                    currentFilterValues = originalFilterValues.ToDictionary(kvp => kvp.Key, kvp => new List<string>(kvp.Value));
                    PopulateAllDropdowns(currentFilterValues);
                }
                else
                {
                    // Retrieve filtered unique values from the repository.
                    filteredValues = CustomerRepository.GetFilteredUniqueValues(
                        selectedRegion == "All" ? "" : selectedRegion,
                        selectedCluster == "All" ? "" : selectedCluster,
                        selectedTeam == "All" ? "" : selectedTeam,
                        selectedEsa == "All" ? "" : selectedEsa,
                        selectedCustomer == "All" ? "" : selectedCustomer
                    );
                    currentFilterValues = filteredValues;
                    PopulateAllDropdowns(filteredValues);
                    // Restore previous selections if they still exist.
                    MaintainDropdownSelection(RegionDropdown, selectedRegion);
                    MaintainDropdownSelection(ClusterDropdown, selectedCluster);
                    MaintainDropdownSelection(TeamDropdown, selectedTeam);
                    MaintainDropdownSelection(EsaDropdown, selectedEsa);
                    MaintainDropdownSelection(CustomerDropdown, selectedCustomer);
                }

                // If your MainContent implements IFilterable, apply the filters.
                if (MainContent.Content is IFilterable filterableView)
                {
                    filterableView.ApplyFilters(
                        selectedRegion == "All" ? "" : selectedRegion,
                        selectedCluster == "All" ? "" : selectedCluster,
                        selectedTeam == "All" ? "" : selectedTeam,
                        selectedEsa == "All" ? "" : selectedEsa,
                        selectedCustomer == "All" ? "" : selectedCustomer
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating filters: {ex.Message}", "Filter Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isUpdatingFilters = false;
            }
        }

        private void MaintainDropdownSelection(ComboBox dropdown, string selectedValue)
        {
            if (dropdown.Items.Contains(selectedValue))
                dropdown.SelectedItem = selectedValue;
            else
                dropdown.SelectedIndex = 0;
        }

        private void ShowCustomerDetails(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new CustomerDetailsView();
            ApplyCurrentFilters();
        }

        // This method handles the search filtering for the ComboBox.
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox searchBox = sender as TextBox;
            if (searchBox == null) return;

            // Get the parent ComboBox from the Tag property
            ComboBox parentComboBox = searchBox.Tag as ComboBox;
            if (parentComboBox == null) return;

            string key = "";
            switch (parentComboBox.Name)
            {
                case "RegionDropdown":
                    key = "region";
                    break;
                case "ClusterDropdown":
                    key = "cluster";
                    break;
                case "TeamDropdown":
                    key = "team";
                    break;
                case "EsaDropdown":
                    key = "esa";
                    break;
                case "CustomerDropdown":
                    key = "customer";
                    break;
                default:
                    return;
            }

            if (originalFilterValues == null || !originalFilterValues.ContainsKey(key))
                return;

            string searchText = searchBox.Text.Trim();
            List<string> filteredItems;
            if (string.IsNullOrEmpty(searchText))
            {
                // If the search text is empty, use the full list.
                filteredItems = originalFilterValues[key];
            }
            else
            {
                filteredItems = originalFilterValues[key]
                    .Where(item => item.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            parentComboBox.SelectionChanged -= FilterSelectionChanged;
            parentComboBox.Items.Clear();
            parentComboBox.Items.Add("All");
            foreach (var item in filteredItems)
            {
                parentComboBox.Items.Add(item);
            }
            parentComboBox.SelectionChanged += FilterSelectionChanged;

            if (filteredItems.Count == 1)
                parentComboBox.SelectedItem = filteredItems.First();
            else
                parentComboBox.SelectedIndex = 0;

            // Keep the dropdown open so the user sees the filtered list.
            parentComboBox.IsDropDownOpen = true;
        }

        // (Optional) Helper method if needed elsewhere
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        private void ShowShipments(object sender, RoutedEventArgs e)
        {
            // Create a new instance of your ShipmentsView user control
            var shipmentsView = new ShipmentsView();

            // Display it in the MainContent area (the ContentControl)
            MainContent.Content = shipmentsView;
        }

    }
}
