using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;
using ClassLibrary.Classes;

namespace DZ4
{
    public partial class ProductWindow
    {
        private readonly string        _connectionString;
        private readonly List<Storage> _storages;
        private readonly List<int>     _subcategoriesId;
        public Product ResultProduct { get; private set; }

        public ProductWindow()
        {
            InitializeComponent();

            _connectionString = ConfigurationManager.ConnectionStrings["ShopDB"].ConnectionString;
            _storages         = new List<Storage>();
            _subcategoriesId  = new List<int>();

            FillSubcategories();
            FillStorages();
        }

        public ProductWindow(Product product) : this()
        {
            NameTextBox.Text                    = product.Name;
            DescriptionTextBox.Text             = product.Description;
            PriceTextBox.Text                   = product.Price.ToString(CultureInfo.InvariantCulture);
            CountTextBox.Text                   = product.Count.ToString();
            SubcategoriesComboBox.SelectedIndex = _subcategoriesId.IndexOf(product.SubcategoryId);
            StoragesComboBox.SelectedIndex      = _storages.IndexOf(_storages.Find(i => i.Id == product.StorageId));
        }

        private void FillSubcategories()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT Name, ID FROM Subcategories";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SubcategoriesComboBox.Items.Add(reader["Name"].ToString());
                            _subcategoriesId.Add(int.Parse(reader["ID"].ToString()));
                        }
                    }
                }
            }
        }

        private void FillStorages()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql =
                    "SELECT City, Street, BuildingNumber, S.ID FROM  StorageAddresses INNER JOIN Storages S on StorageAddresses.ID = S.StorageAddressID WHERE S.ID = StorageAddressID";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var storage = new Storage
                            {
                                Id             = int.Parse(reader["ID"].ToString()),
                                City           = reader["City"].ToString(),
                                BuildingNumber = reader["BuildingNumber"].ToString(),
                                Street         = reader["Street"].ToString()
                            };

                            _storages.Add(storage);

                            StoragesComboBox.Items.Add($"{storage.City} {storage.Street} {storage.BuildingNumber}");
                        }
                    }
                }
            }
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsCorrectInputData())
                return;

            ResultProduct = new Product
            {
                Name          = NameTextBox.Text,
                Description   = DescriptionTextBox.Text,
                Price         = double.Parse(PriceTextBox.Text),
                Count         = int.Parse(CountTextBox.Text),
                StorageId     = _storages[StoragesComboBox.SelectedIndex].Id,
                SubcategoryId = GetSubcategoryId()
            };

            MessageBox.Show("Changes applied");

            DialogResult = true;
            Close();
        }

        private bool IsCorrectInputData()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PriceTextBox.Text) ||
                string.IsNullOrWhiteSpace(CountTextBox.Text) ||
                SubcategoriesComboBox.SelectedIndex == -1 ||
                StoragesComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("You need to fill name, price and count fields and pick subcategory " +
                                "and storage.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!double.TryParse(PriceTextBox.Text, out var _) ||
                !int.TryParse(CountTextBox.Text, out var _))
            {
                MessageBox.Show("You entered incorrect format of price or count.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private int GetSubcategoryId()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT ID FROM Subcategories WHERE Name LIKE @name";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name",
                        SubcategoriesComboBox.SelectedItem.ToString());

                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();

                        return int.Parse(reader["ID"].ToString());
                    }
                }
            }
        }
    }
}