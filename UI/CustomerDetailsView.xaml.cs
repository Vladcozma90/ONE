using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ONE.UI
{
    public interface IFilterable
    {
        void ApplyFilters(string region, string cluster, string team, string esa, string customer);
    }

    public partial class CustomerDetailsView : UserControl, IFilterable 
    {
        private string currentCustomer_name = "";
        private bool isEditing = false;

        public CustomerDetailsView()
        {
            InitializeComponent();
            ShowPlaceHolder();
        }

        private void LoadCustomer(string region = "", string cluster = "", string team = "", string esa = "", string Customer_name = "")
        {
            if (string.IsNullOrEmpty(region+cluster+team+esa+Customer_name))
            {
                ShowPlaceHolder();
                return;
            }

            var customers = CustomerRepository.GetCustomersFiltered(region, cluster, team, esa, Customer_name);

            if (customers.Count == 0)
            {
                ShowPlaceHolder();
                return;
            }

            var customer = customers[0];
            currentCustomer_name = customer.Name;
                       

            Customer_nameValue.Text = customer.Name;
            ContactValue.Text = customer.Contact;
            BillToValue.Text = customer.BillTo.ToString();
            ShipToValue.Text = customer.ShipTo.ToString();
            DCodeValue.Text = customer.DCode;
            IncotermValue.Text = customer.Incoterm;
            DestinationValue.Text = customer.Destination;
            CountryValue.Text = customer.Country;
            FreightCostValue.Text = customer.FreightCost;

            NotesTextBox.Text = CustomerRepository.GetCustomerNotes(currentCustomer_name);


            ToggleVisibility(true);
            SetEditMode(false);   

        }

        private void ShowPlaceHolder() => ToggleVisibility(false);

        private void ToggleVisibility(bool show)
        {
            CustomerDetailsGrid.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            CustomerInfoGrid.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            NotesSection.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            PlaceholderMessage.Visibility = show ? Visibility.Collapsed : Visibility.Visible;

        }

        public void ApplyFilters(string region, string cluster, string team, string esa, string customer)
        {
            LoadCustomer(
                region == "All" ? "" : region,
                cluster == "All" ? "" : cluster,
                team == "All" ? "" : team,
                esa == "All" ? "" : esa,
                customer == "All" ? "" : customer
                );
        }

        private void ToggleEditMode(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentCustomer_name)) return;

            isEditing = !isEditing;
            SetEditMode(isEditing);

            if (!isEditing)
            {
                CustomerRepository.SaveCustomerNotes(currentCustomer_name, NotesTextBox.Text);
                MessageBox.Show("Notes saved succesfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SetEditMode(bool enable)
        {
            NotesTextBox.IsReadOnly = !enable;
            NotesTextBox.Background = enable ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.LightGray;
            EditSaveButton.Content = enable ? "Save" : "Edit";
        }
    }
}