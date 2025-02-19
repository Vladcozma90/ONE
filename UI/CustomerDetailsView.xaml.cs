using System.Windows;
using System.Windows.Controls;

namespace ONE.UI
{
    public partial class CustomerDetailsView : UserControl
    {
        public CustomerDetailsView()
        {
            InitializeComponent();
            ShowCustomerDetails();
        }

        private void ShowCustomerDetails()
        {
            CustomerDetailsGrid.Visibility = Visibility.Visible;
            CustomerInfoGrid.Visibility = Visibility.Visible;
            NotesSection.Visibility = Visibility.Visible;
        }
    }
}
