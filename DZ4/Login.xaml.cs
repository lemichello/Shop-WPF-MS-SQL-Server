using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using ClassLibrary.Classes;

namespace DZ4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly string     _connectionString;
        private readonly List<User> _users;

        public MainWindow()
        {
            InitializeComponent();

            _connectionString = ConfigurationManager.ConnectionStrings["ShopDB"].ConnectionString;
            _users            = new List<User>();

            FillUsersList();
        }

        private void EnterButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordTextBox.Password))
            {
                MessageBox.Show("You need to fill login and password fields.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = _users.FirstOrDefault(i => i.Login == LoginTextBox.Text &&
                                                  i.Password == PasswordTextBox.Password);

            if (user == null)
            {
                MessageBox.Show("You entered incorrect login or password. Try again.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Window window = null;

            switch (user.Role)
            {
                case UserRoles.Admin:
                    window = new AdminWindow();
                    break;

                case UserRoles.Manager:
                    window = new ManagerWindow();
                    break;

                case UserRoles.User:
                    window = new UserWindow(user.Id);
                    break;
            }

            Close();
            window?.ShowDialog();
        }

        private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new RegisterWindow();

            Visibility = Visibility.Collapsed;

            if (window.ShowDialog() == true)
            {
                _users.Clear();
                FillUsersList();
            }

            Visibility = Visibility.Visible;
        }

        private void FillUsersList()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM Users";

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _users.Add(GetUser(reader));
                        }
                    }
                }
            }
        }

        private User GetUser(SqlDataReader reader)
        {
            var user = new User
            {
                Id          = int.Parse(reader["ID"].ToString()),
                FirstName   = reader["FirstName"].ToString(),
                LastName    = reader["LastName"].ToString(),
                Email       = reader["Email"].ToString(),
                PhoneNumber = reader["PhoneNumber"].ToString(),
                Login       = reader["Login"].ToString(),
                Password    = reader["Password"].ToString()
            };

            user.Role = GetUserRole(user.Id);

            return user;
        }

        private UserRoles GetUserRole(int userId)
        {
            var role = UserRoles.None;
            const string sql =
                "SELECT Name FROM Roles INNER JOIN UsersRoles UR on Roles.ID = UR.RoleID WHERE UR.UserID = @userId";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            role = (UserRoles) Enum.Parse(typeof(UserRoles), reader["Name"].ToString());
                        }
                    }
                }
            }

            return role;
        }
    }
}