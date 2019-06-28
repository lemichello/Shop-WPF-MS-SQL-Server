using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using ClassLibrary.Classes;

namespace DZ4
{
    public partial class AddSubcategoryWindow
    {
        private readonly string _connectionString;

        public AddSubcategoryWindow()
        {
            InitializeComponent();

            _connectionString = ConfigurationManager.ConnectionStrings["ShopDB"].ConnectionString;

            FillCategories();
        }

        private void FillCategories()
        {
            UserWindow.FillCategoriesComboBox(CategoriesComboBox.Items, _connectionString);
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SubcategoryTextBox.Text) ||
                CategoriesComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("You need to fill the subcategory name field and pick a category, " +
                                "to which new subcategory will belong.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SubcategoryExists())
            {
                MessageBox.Show("This subcategory already exists. Try another name.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AddSubcategory();

            MessageBox.Show("Adding successfully completed.");

            Close();
        }

        private bool SubcategoryExists()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT ID FROM Subcategories WHERE Name LIKE @subcategory";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@subcategory", SubcategoryTextBox.Text);

                    using (var reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }

        private void AddSubcategory()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "INSERT INTO Subcategories(Name, CategoryID) VALUES (@name, @categoryId)";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", SubcategoryTextBox.Text);
                    command.Parameters.AddWithValue("@categoryId", GetCategoryId());

                    command.ExecuteNonQuery();
                }
            }
        }

        private int GetCategoryId()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT ID FROM Categories WHERE Name LIKE @name";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name",
                        CategoriesComboBox.SelectedItem.ToString());

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