using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;

namespace DZ4
{
    public partial class RegisterWindow
    {
        private readonly string _connectionString;

        public RegisterWindow()
        {
            InitializeComponent();

            _connectionString = ConfigurationManager.ConnectionStrings["ShopDB"].ConnectionString;
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsInputDataValid())
                return;

            if (LoginExists())
            {
                MessageBox.Show("This login already exists. Try another.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql =
                    "INSERT INTO Users(FirstName, LastName, Email, PhoneNumber, Login, Password) OUTPUT inserted.ID VALUES (@firstName, @lastName, @email, @number, @login, @password)";

                connection.Open();

                int id;

                using (var command = new SqlCommand(sql, connection))
                {
                    FillParameters(command);

                    id = (int) command.ExecuteScalar();
                }

                AddRoleToUser(id, connection);
                AddToCartAndWishList(id, connection);
            }

            MessageBox.Show("Registration complete.");
            DialogResult = true;
            Close();
        }

        private void FillParameters(SqlCommand command)
        {
            command.Parameters.AddWithValue("@firstName", FirstNameTextBox.Text);
            command.Parameters.AddWithValue("@lastName", LastNameTextBox.Text);
            command.Parameters.AddWithValue("@email", EmailTextBox.Text);
            command.Parameters.AddWithValue("@number", PhoneNumberTextBox.Text);
            command.Parameters.AddWithValue("@login", LoginTextBox.Text);
            command.Parameters.AddWithValue("@password", PasswordTextBox.Password);
        }

        private bool LoginExists()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM Users WHERE Login = @login";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@login", LoginTextBox.Text);

                    return command.ExecuteReader().HasRows;
                }
            }
        }

        private bool IsEmptyFields()
        {
            return string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                   string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                   string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                   string.IsNullOrWhiteSpace(PhoneNumberTextBox.Text) ||
                   string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                   string.IsNullOrWhiteSpace(PasswordTextBox.Password);
        }

        private bool IsEmailValid()
        {
            try
            {
                var _ = new MailAddress(EmailTextBox.Text);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool IsInputDataValid()
        {
            if (IsEmptyFields())
            {
                MessageBox.Show("You need to fill all fields.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!IsEmailValid())
            {
                MessageBox.Show("You entered incorrect email.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Regex.IsMatch(PhoneNumberTextBox.Text, @"^\d{10}$"))
            {
                MessageBox.Show("You entered incorrect phone number.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private static void AddRoleToUser(long id, SqlConnection connection)
        {
            const string sql = "INSERT INTO UsersRoles(RoleID, UserID) VALUES (1, @id)";

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                command.ExecuteNonQuery();
            }
        }

        private static void AddToCartAndWishList(long id, SqlConnection connection)
        {
            var sql = "SET IDENTITY_INSERT Cart ON; INSERT INTO Cart(ID) VALUES (@id); SET IDENTITY_INSERT Cart OFF";

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                command.ExecuteNonQuery();
            }

            sql =
                "SET IDENTITY_INSERT WishList ON; INSERT INTO WishList(ID) VALUES (@id); SET IDENTITY_INSERT WishList OFF;";

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                command.ExecuteNonQuery();
            }
        }
    }
}