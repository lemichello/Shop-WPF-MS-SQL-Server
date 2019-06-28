using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClassLibrary.Classes;

namespace DZ4
{
    public partial class ManagerWindow
    {
        private readonly string                                  _connectionString;
        private readonly ObservableCollection<ProductForManager> _products;
        private          List<ProductForManager>                 _filteredProducts;

        public ManagerWindow()
        {
            InitializeComponent();

            _connectionString = ConfigurationManager.ConnectionStrings["ShopDB"].ConnectionString;
            _products         = new ObservableCollection<ProductForManager>();
            _filteredProducts = new List<ProductForManager>();

            StatisticTypes.Items.Add("Products, that are going to end (count on storage < 10)");
            StatisticTypes.Items.Add("Most popular 10 products");
            StatisticTypes.Items.Add("All products");

            ProductsDataGrid.ItemsSource = _products;
        }

        private void StatisticTypes_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (StatisticTypes.SelectedIndex)
            {
                case 0:
                    FillProductsThatAreGoingToEnd();
                    break;

                case 1:
                    FillProductsByPopularity();
                    break;

                case 2:
                    FillAllProducts();
                    break;
            }

            SearchTextBox.Visibility = Visibility.Visible;
        }

        private void FillProductsThatAreGoingToEnd()
        {
            _products.Clear();

            // CountInOrders.
            ProductsDataGrid.Columns[4].Visibility = Visibility.Collapsed;
            // CountInCarts.
            ProductsDataGrid.Columns[5].Visibility = Visibility.Collapsed;

            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM Products WHERE Count < 10";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _products.Add(GetProduct(reader));
                        }
                    }
                }
            }
        }

        private void FillProductsByPopularity()
        {
            _products.Clear();

            ProductsDataGrid.Columns[4].Visibility = Visibility.Visible;
            ProductsDataGrid.Columns[5].Visibility = Visibility.Visible;

            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM SelectByPopularity";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var countInCarts  = reader["CountInCarts"].ToString();
                            var countInOrders = reader["CountInOrders"].ToString();

                            var product = GetProduct(reader);

                            product.CountInOrders = int.Parse(countInOrders == "" ? "0" : countInOrders);
                            product.CountInCarts  = int.Parse(countInCarts == "" ? "0" : countInCarts);

                            _products.Add(product);
                        }
                    }
                }
            }
        }

        private void FillAllProducts()
        {
            _products.Clear();

            ProductsDataGrid.Columns[4].Visibility = Visibility.Collapsed;
            ProductsDataGrid.Columns[5].Visibility = Visibility.Collapsed;

            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM Products";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _products.Add(GetProduct(reader));
                        }
                    }
                }
            }
        }

        private static ProductForManager GetProduct(SqlDataReader reader)
        {
            return new ProductForManager
            {
                Id          = int.Parse(reader["ID"].ToString()),
                Name        = reader["Name"].ToString(),
                Description = reader["Description"].ToString(),
                Count       = int.Parse(reader["Count"].ToString()),
                Price       = double.Parse(reader["Price"].ToString())
            };
        }

        private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ProductsDataGrid.ItemsSource = null;

            if (SearchTextBox.Text == "")
            {
                ProductsDataGrid.ItemsSource = _products;
                return;
            }

            _filteredProducts = _products.Where(i => i.Name.Contains(SearchTextBox.Text)).ToList();

            ProductsDataGrid.ItemsSource = _filteredProducts;
        }
    }
}