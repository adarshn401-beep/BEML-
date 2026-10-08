using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using BEMLPropertyManagement.Data;

namespace BEMLPropertyManagement;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        string username = UsernameTextBox.Text.Trim();
        string password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            MessageTextBlock.Text = "Please enter username and password.";
            return;
        }

        try
        {
            string passwordHash = CreatePasswordHash(password);

            using var db = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(
                        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;")
                    .Options);

            var user = db.Users
                .FirstOrDefault(u =>
                    u.Username == username &&
                    u.PasswordHash == passwordHash &&
                    u.IsActive);

            if (user == null)
            {
                MessageTextBlock.Text = "Invalid username or password.";
                return;
            }

            MessageBox.Show(
                $"Welcome, {user.FullName}!",
                "Login Successful",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            MainWindow mainWindow = new MainWindow();

            Application.Current.MainWindow = mainWindow;

            mainWindow.Show();

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to connect to the database.\n\n" + ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static string CreatePasswordHash(string password)
    {
        using SHA256 sha256 = SHA256.Create();

        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = sha256.ComputeHash(bytes);

        return Convert.ToHexString(hash);
    }
}