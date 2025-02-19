using System.Windows;
using System.Windows.Controls;
using ONE.UI;  // Ensure this namespace is correct

namespace ONE
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = null; // Start with an empty screen
        }

        private void ShowCustomerDetails(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new CustomerDetailsView(); // Load Customer Details when clicked
        }

        private void FilterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainContent.Content is IFilterable filterableView)
            {
                filterableView.ApplyFilters(
                    RegionDropdown.SelectedItem,
                    ClusterDropdown.SelectedItem,
                    TeamDropdown.SelectedItem,
                    ESADropdown.SelectedItem,
                    CustomerDropdown.SelectedItem
                );
            }
        }
    }

    public interface IFilterable
    {
        void ApplyFilters(object region, object cluster, object team, object esa, object customer);
    }
}
