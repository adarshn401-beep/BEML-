using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BEMLPropertyManagement.Data;
using BEMLPropertyManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class UsersWindow : Window
{
    public UsersWindow()
    {
        InitializeComponent();
        LoadUsers();
    }

    private void LoadUsers(string searchText = "")
    {
        try
        {
            using var db = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(
                        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;")
                    .Options);

            var users = db.Users
                .AsNoTracking()
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.FullName,
                    u.Role,
                    EmployeeName = u.EmployeeId == null
                        ? ""
                        : db.Employees
                            .Where(e => e.EmployeeId == u.EmployeeId)
                            .Select(e => e.EmployeeName)
                            .FirstOrDefault() ?? "",
                    Status = u.IsActive ? "Active" : "Inactive",
                    u.IsActive
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim().ToLower();

                users = users
                    .Where(u =>
                        u.Username.ToLower().Contains(searchText) ||
                        u.FullName.ToLower().Contains(searchText) ||
                        u.Role.ToLower().Contains(searchText) ||
                        u.EmployeeName.ToLower().Contains(searchText) ||
                        u.Status.ToLower().Contains(searchText))
                    .ToList();
            }

            UsersDataGrid.ItemsSource = users;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load users.\n\n" + ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddUserButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Add User screen will be added in the next step.",
            "User Management",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        string searchText = SearchTextBox.Text.Trim();

        if (searchText == "Search users...")
        {
            searchText = "";
        }

        LoadUsers(searchText);
    }

    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchButton_Click(sender, e);
        }
    }

    private void ViewUserButton_Click(object sender, RoutedEventArgs e)
    {
        if (UsersDataGrid.SelectedItem == null)
        {
            if (sender is Button button &&
                button.DataContext != null)
            {
                ShowUserDetails(button.DataContext);
            }

            return;
        }

        ShowUserDetails(UsersDataGrid.SelectedItem);
    }

    private void ShowUserDetails(object user)
    {
        var userType = user.GetType();

        string username =
            userType.GetProperty("Username")?.GetValue(user)?.ToString() ?? "";

        string fullName =
            userType.GetProperty("FullName")?.GetValue(user)?.ToString() ?? "";

        string role =
            userType.GetProperty("Role")?.GetValue(user)?.ToString() ?? "";

        string employee =
            userType.GetProperty("EmployeeName")?.GetValue(user)?.ToString() ?? "";

        string status =
            userType.GetProperty("Status")?.GetValue(user)?.ToString() ?? "";

        MessageBox.Show(
            $"Username: {username}\n\n" +
            $"Full Name: {fullName}\n\n" +
            $"Role: {role}\n\n" +
            $"Employee: {employee}\n\n" +
            $"Status: {status}",
            "User Details",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void EditUserButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.DataContext == null)
        {
            return;
        }

        MessageBox.Show(
            "Edit User screen will be added in the next step.",
            "User Management",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.DataContext == null)
        {
            return;
        }

        var userType = button.DataContext.GetType();

        int userId = Convert.ToInt32(
            userType.GetProperty("UserId")?.GetValue(button.DataContext));

        string username =
            userType.GetProperty("Username")?.GetValue(button.DataContext)?.ToString() ?? "";

        if (username.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(
                "The main admin account cannot be deleted.",
                "Delete User",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        MessageBoxResult result = MessageBox.Show(
            $"Are you sure you want to delete user '{username}'?",
            "Delete User",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            using var db = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(
                        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;")
                    .Options);

            User? user = db.Users
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                MessageBox.Show(
                    "User was not found.",
                    "Delete User",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            user.IsActive = false;

            db.SaveChanges();

            MessageBox.Show(
                "User has been deactivated successfully.",
                "User Management",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadUsers();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to deactivate user.\n\n" + ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}