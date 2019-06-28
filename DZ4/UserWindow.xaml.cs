using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClassLibrary.Classes;
using MaterialDesignThemes.Wpf;

namespace DZ4
{
    public partial class UserWindow
    {
        private readonly List<Product>                 _products;
        private readonly ObservableCollection<Product> _cartProducts;
        private readonly ObservableCollection<string>  _categories;
        private readonly ObservableCollection<string>  _subcategories;
        private readonly string                        _connectionString;
        private readonly int                           _userId;

        public UserWindow(int id)
        {
            InitializeComponent();

            _connectionString = ConfigurationManager.ConnectionStrings["ShopDB"].ConnectionString;
            _userId           = id;
            _products         = new List<Product>();
            _cartProducts     = new ObservableCollection<Product>();
            _categories       = new ObservableCollection<string>();
            _subcategories    = new ObservableCollection<string>();


            FillCategoriesComboBox(_categories, _connectionString);
            FillCartData();

            CartItems.ItemsSource             = _cartProducts;
            CategoriesComboBox.ItemsSource    = _categories;
            SubCategoriesComboBox.ItemsSource = _subcategories;
        }

        public static void FillCategoriesComboBox(IList collection, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                const string sql = "SELECT Name FROM Categories";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            collection.Add(reader["Name"].ToString());
                        }
                    }
                }
            }
        }

        private void FillCartData()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql =
                    "SELECT p.ID, p.Name, p.Price, CP.Count FROM Products p INNER JOIN CartsProducts CP on p.ID = CP.ProductID INNER JOIN Cart C on CP.CartID = C.ID WHERE CP.ProductID = p.ID AND C.ID = @id";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("id", _userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _cartProducts.Add(new Product
                            {
                                Id    = int.Parse(reader["ID"].ToString()),
                                Name  = reader["Name"].ToString(),
                                Count = int.Parse(reader["Count"].ToString()),
                                Price = double.Parse(reader["Price"].ToString())
                            });
                        }
                    }
                }
            }

            if (_cartProducts.Count != 0)
                MakeOrderButton.Visibility = Visibility.Visible;
        }

        private void FillProductsData(string category)
        {
            _products.Clear();

            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql =
                    "SELECT * FROM  Products WHERE SubcategoryID = (SELECT ID FROM Subcategories WHERE Name LIKE @category)";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@category", category);

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

        private void ProductsDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsDataGrid.SelectedIndex == -1)
            {
                AddButton.Visibility               = Visibility.Collapsed;
                CountControlsStackPanel.Visibility = Visibility.Collapsed;

                return;
            }

            AddButton.Visibility               = Visibility.Visible;
            CountControlsStackPanel.Visibility = Visibility.Visible;
            ProductsCountLabel.Content         = "1";
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedProduct = (Product) ProductsDataGrid.SelectedItem;
            var cartProduct = new Product
            {
                Id    = selectedProduct.Id,
                Name  = selectedProduct.Name,
                Count = int.Parse(ProductsCountLabel.Content.ToString()),
                Price = selectedProduct.Price
            };

            _cartProducts.Add(cartProduct);
            InsertProductToCart(cartProduct.Id, cartProduct.Count);

            MakeOrderButton.Visibility = Visibility.Visible;
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedProduct = _cartProducts[CartItems.SelectedIndex];

            _cartProducts.Remove(selectedProduct);
            RemoveProductFromCart(selectedProduct);
        }

        private void RemoveProductFromCart(Product selectedProduct)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "DELETE FROM CartsProducts WHERE CartID = @cartId AND ProductID = @productId";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@cartId", _userId);
                    command.Parameters.AddWithValue("@productId", selectedProduct.Id);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void InsertProductToCart(int productId, int productCount)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql =
                    "INSERT INTO CartsProducts(CartID, ProductID, Count) VALUES (@cartId, @productId, @count)";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@cartId", _userId);
                    command.Parameters.AddWithValue("@productId", productId);
                    command.Parameters.AddWithValue("@count", productCount);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void SubCategoriesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SubCategoriesComboBox.SelectedIndex != -1)
                FillProductsData(SubCategoriesComboBox.SelectedItem.ToString());
        }

        private void CategoriesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriesComboBox.SelectedIndex == -1) return;

            FillSubcategoriesComboBox(CategoriesComboBox.SelectedItem.ToString());
            ProductsDataGrid.ItemsSource = null;
        }

        private void FillSubcategoriesComboBox(string category)
        {
            _subcategories.Clear();

            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql =
                    "SELECT Name FROM Subcategories WHERE CategoryID = (SELECT ID FROM Categories WHERE Name LIKE @category)";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@category", category);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _subcategories.Add(reader["Name"].ToString());
                        }
                    }
                }
            }
        }

        private void IncreaseCountButton_OnClick(object sender, RoutedEventArgs e)
        {
            ProductsCountLabel.Content = int.Parse(ProductsCountLabel.Content.ToString()) + 1;
        }

        private void DecreaseCountButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ProductsCountLabel.Content.ToString() != "1")
                ProductsCountLabel.Content = int.Parse(ProductsCountLabel.Content.ToString()) - 1;
        }

        private void MakeOrderButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new OrderWindow(_userId, _cartProducts);

            if (window.ShowDialog() == true)
                MakeOrderButton.Visibility = Visibility.Collapsed;
        }

        private void CartItems_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveButton.Visibility = CartItems.SelectedIndex == -1
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}