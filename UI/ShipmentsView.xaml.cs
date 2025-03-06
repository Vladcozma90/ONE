using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ONE.UI
{
    public partial class ShipmentsView : UserControl
    {
        // Collection for rows (each row is a ShipmentRow)
        public ObservableCollection<ShipmentRow> Shipments { get; set; }
        // Collection for column header definitions (editable titles)
        public ObservableCollection<ColumnHeader> ColumnHeaders { get; set; }

        public ShipmentsView()
        {
            InitializeComponent();
            Shipments = new ObservableCollection<ShipmentRow>();
            ColumnHeaders = new ObservableCollection<ColumnHeader>();

            // Initialize with two columns
            AddNewColumn();
            AddNewColumn();

            // Bind the DataGrid to the rows collection
            ShipmentsDataGrid.ItemsSource = Shipments;
            UpdateColumns();
        }

        // Event handler: Add a new row (appended at the bottom)
        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            var newRow = new ShipmentRow(ColumnHeaders.Count);
            Shipments.Add(newRow);
        }

        // Event handler: Add a new column (appended at the right)
        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            AddNewColumn();
            UpdateColumns();
        }

        // Create a new column header and update each existing row with an empty cell.
        private void AddNewColumn()
        {
            int newIndex = ColumnHeaders.Count;
            ColumnHeaders.Add(new ColumnHeader { Title = $"Column {newIndex + 1}" });

            // For each existing row, add an empty cell for the new column.
            foreach (var row in Shipments)
            {
                row.AddColumn();
            }
        }

        // Rebuild DataGrid columns based on the current ColumnHeaders.
        private void UpdateColumns()
        {
            ShipmentsDataGrid.Columns.Clear();
            for (int i = 0; i < ColumnHeaders.Count; i++)
            {
                var col = new DataGridTextColumn
                {
                    Header = CreateEditableHeader(i),
                    Binding = new Binding($"Columns[{i}]") { Mode = BindingMode.TwoWay }
                };
                ShipmentsDataGrid.Columns.Add(col);
            }
        }

        // Create an editable header as a TextBox for the column at the specified index.
        private TextBox CreateEditableHeader(int columnIndex)
        {
            var tb = new TextBox
            {
                Text = ColumnHeaders[columnIndex].Title,
                MinWidth = 100,
                MaxWidth = 150
            };
            tb.LostFocus += (s, e) =>
            {
                ColumnHeaders[columnIndex].Title = tb.Text;
                ShipmentsDataGrid.Columns[columnIndex].Header = tb;
            };
            return tb;
        }
    }

    // Model representing one row in the spreadsheet.
    public class ShipmentRow : INotifyPropertyChanged
    {
        public ObservableCollection<string> Columns { get; set; }

        public ShipmentRow(int columnCount)
        {
            Columns = new ObservableCollection<string>();
            for (int i = 0; i < columnCount; i++)
            {
                Columns.Add(string.Empty);
            }
        }

        // Adds an empty cell to the row for a new column.
        public void AddColumn()
        {
            Columns.Add(string.Empty);
            OnPropertyChanged("Columns");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Model representing a column header with an editable title.
    public class ColumnHeader : INotifyPropertyChanged
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
