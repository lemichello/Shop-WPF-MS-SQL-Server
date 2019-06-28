using System.Configuration;
using System.Data.SqlClient;
using System.Windows;

namespace DZ4
{
    public partial class AddCategoryWindow
    {
        private readonly string _connectionString;

        public AddCategoryWindow()
        {
            InitializeComponent();

            _connectionString = ConfigurationManager.ConnectionStrings["ShopDB"].ConnectionString;
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryTextBox.Text))
            {
                MessageBox.Show("You need to fill category name field.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CategoryExists())
            {
                MessageBox.Show("This category already exists. Try another name.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AddCategory();

            MessageBox.Show("Adding successfully completed.");

            Close();
        }

        private bool CategoryExists()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT ID FROM Categories WHERE Name LIKE @category";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@category", CategoryTextBox.Text);

                    using (var reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }

        private void AddCategory()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "INSERT INTO Categories(Name) VALUES (@category)";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@category", CategoryTextBox.Text);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}