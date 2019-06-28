using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using ClassLibrary.Classes;

namespace DZ4
{
    public partial class OrderWindow : Window
    {
        private readonly int                           _userId;
        private readonly string                        _connectionString;
        private readonly ObservableCollection<Product> _cartProducts;

        public OrderWindow(int userId, ObservableCollection<Product> cartProducts)
        {
            InitializeComponent();

            _userId           = userId;
            _connectionString = ConfigurationManager.ConnectionStrings["ShopDB"].ConnectionString;
            _cartProducts     = cartProducts;

            FillPaymentsMethods();
            FillDeliveryMethods();

            PriceLabel.Content = $"{PriceLabel.Content}{CalculateTotalPrice()} UAH";
        }

        private double CalculateTotalPrice()
        {
            return _cartProducts.Sum(i => i.Price * i.Count);
        }

        private void FillPaymentsMethods()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT Method FROM PaymentMethods";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PaymentMethodsBox.Items.Add(reader["Method"].ToString());
                        }
                    }
                }
            }
        }

        private void FillDeliveryMethods()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT Method FROM DeliveryMethods";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DeliveryMethodsBox.Items.Add(reader["Method"].ToString());
                        }
                    }
                }
            }
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (DeliveryMethodsBox.SelectedIndex == -1 ||
                PaymentMethodsBox.SelectedIndex == -1)
            {
                MessageBox.Show("You need to pick delivery and payment methods.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var orderId = InsertOrder(connection);

                InsertCartProducts(connection, orderId);
            }

            DialogResult = true;
            Close();
        }

        private int InsertOrder(SqlConnection connection)
        {
            const string sql =
                "INSERT INTO Orders(PaymentMethodID, UserID, DeliveryMethodID) OUTPUT inserted.ID VALUES (@payment, @userId, @delivery)";

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@payment", GetPaymentId());
                command.Parameters.AddWithValue("@userId", _userId);
                command.Parameters.AddWithValue("@delivery", GetDeliveryId());

                return (int) command.ExecuteScalar();
            }
        }

        private void InsertCartProducts(SqlConnection connection, int orderId)
        {
            const string sql =
                "INSERT INTO ProductsOrders(OrderID, ProductID, Count) VALUES (@orderId, @productId, @count)";


            foreach (var product in _cartProducts)
            {
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@orderId", orderId);
                    command.Parameters.AddWithValue("@productId", product.Id);
                    command.Parameters.AddWithValue("@count", product.Count);

                    command.ExecuteNonQuery();
                }
            }

            RemoveProductsFromCart();
            _cartProducts.Clear();
        }

        private int GetPaymentId()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT ID FROM PaymentMethods WHERE Method LIKE @payment";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@payment",
                        PaymentMethodsBox.SelectedItem.ToString());

                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();

                        return int.Parse(reader["ID"].ToString());
                    }
                }
            }
        }

        private int GetDeliveryId()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT ID FROM DeliveryMethods WHERE Method LIKE @delivery";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@delivery",
                        DeliveryMethodsBox.SelectedItem.ToString());

                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();

                        return int.Parse(reader["ID"].ToString());
                    }
                }
            }
        }

        private void RemoveProductsFromCart()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "DELETE FROM CartsProducts WHERE CartID = @id";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", _userId);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}