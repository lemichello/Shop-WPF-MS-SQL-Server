using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using ClassLibrary.Classes;

namespace DZ4
{
    public partial class AdminWindow
    {
        private readonly string                        _connectionString;
        private readonly ObservableCollection<Product> _products;

        public AdminWindow()
        {
            InitializeComponent();

            _connectionString = ConfigurationManager.ConnectionStrings["ShopDB"].ConnectionString;
            _products         = new ObservableCollection<Product>();

            FillProductsDataGrid();
        }

        private void FillProductsDataGrid()
        {
            _products.Clear();

            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql =
                    "SELECT ID, Name, Description, Price, Count, SubcategoryID, StorageID FROM  Products";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _products.Add(Product.GetProduct(reader));
                        }
                    }

                    ProductsDataGrid.ItemsSource = null;
                    ProductsDataGrid.ItemsSource = _products;
                }
            }
        }

        private void AddCategoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new AddCategoryWindow();

            window.ShowDialog();
        }

        private void AddSubcategoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new AddSubcategoryWindow();

            window.ShowDialog();
        }

        private void ProductsDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsDataGrid.SelectedIndex == -1)
            {
                DeleteButton.Visibility = Visibility.Collapsed;
                EditButton.Visibility   = Visibility.Collapsed;
                AddButton.Margin        = new Thickness(0, 10, 0, 10);

                return;
            }

            // Show edit and delete buttons when admin selected the product.
            DeleteButton.Visibility = Visibility.Visible;
            EditButton.Visibility   = Visibility.Visible;
            AddButton.Margin        = new Thickness(0, 10, 0, 0);
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new ProductWindow();

            if (window.ShowDialog() == false)
                return;

            AddProduct(window.ResultProduct);
            _products.Add(window.ResultProduct);
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            var editingProduct = (Product) ProductsDataGrid.SelectedItem;
            var window         = new ProductWindow(editingProduct);

            if (window.ShowDialog() == false)
                return;

            UpdateProduct(window.ResultProduct);
            _products[_products.IndexOf(editingProduct)] = window.ResultProduct;
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var deletingProduct = (Product) ProductsDataGrid.SelectedItem;

            DeleteProduct(deletingProduct.Id);
            _products.Remove(deletingProduct);
        }

        private void AddProduct(Product product)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql =
                    "INSERT INTO Products(Name, Description, Price, Count, SubcategoryID, StorageID) VALUES (@name, @description, @price, @count, @subcategory, @storage)";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    FillCommandParameters(command, product);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void UpdateProduct(Product product)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql =
                    "UPDATE Products SET Name = @name, Description = @description, Price = @price, Count = @count, SubcategoryID = @subcategory, StorageID = @storage WHERE ID = @id";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", product.Id);
                    FillCommandParameters(command, product);

                    command.ExecuteNonQuery();
                }
            }
        }

        private static void FillCommandParameters(SqlCommand command, Product product)
        {
            command.Parameters.AddWithValue("@name", product.Name);
            command.Parameters.AddWithValue("@description", product.Description);
            command.Parameters.AddWithValue("@price", product.Price);
            command.Parameters.AddWithValue("@count", product.Count);
            command.Parameters.AddWithValue("@subcategory", product.SubcategoryId);
            command.Parameters.AddWithValue("@storage", product.StorageId);
        }

        private void DeleteProduct(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "DELETE FROM Products WHERE ID = @id";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}