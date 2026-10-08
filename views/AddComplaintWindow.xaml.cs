using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BEMLPropertyManagement.Data;
using BEMLPropertyManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class AddComplaintWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public AddComplaintWindow()
    {
        InitializeComponent();

        LoadAssets();
        LoadEmployees();

        ComplaintDatePicker.SelectedDate = DateTime.Today;
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private void LoadAssets()
    {
        try
        {
            using var db = CreateDbContext();

            AssetComboBox.ItemsSource = db.Assets
                .OrderBy(x => x.ItemNumber)
                .ToList();

            AssetComboBox.DisplayMemberPath = "ItemNumber";
            AssetComboBox.SelectedValuePath = "AssetId";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load assets.\n\n" + ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void LoadEmployees()
    {
        try
        {
            using var db = CreateDbContext();

            EmployeeComboBox.ItemsSource = db.Employees
                .Where(x => x.IsActive)
                .OrderBy(x => x.EmployeeName)
                .ToList();

            EmployeeComboBox.DisplayMemberPath = "EmployeeName";
            EmployeeComboBox.SelectedValuePath = "EmployeeId";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load employees.\n\n" + ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SaveComplaintButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            if (AssetComboBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Please select an asset.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (ComplaintDatePicker.SelectedDate == null)
            {
                MessageBox.Show(
                    "Please select Complaint Date.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(ProblemTitleTextBox.Text))
            {
                MessageBox.Show(
                    "Please enter Problem Title.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    ProblemDescriptionTextBox.Text))
            {
                MessageBox.Show(
                    "Please enter Problem Description.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string priority = "Medium";

            if (PriorityComboBox.SelectedItem is ComboBoxItem priorityItem)
            {
                priority =
                    priorityItem.Content?.ToString() ?? "Medium";
            }

            string status = "Open";

            if (StatusComboBox.SelectedItem is ComboBoxItem statusItem)
            {
                status =
                    statusItem.Content?.ToString() ?? "Open";
            }

            using var db = CreateDbContext();

            var complaint = new Complaint
            {
                AssetId = (int)AssetComboBox.SelectedValue,

                EmployeeId = EmployeeComboBox.SelectedValue != null
                    ? (int?)EmployeeComboBox.SelectedValue
                    : null,

                ComplaintDate =
                    ComplaintDatePicker.SelectedDate.Value,

                ProblemTitle =
                    ProblemTitleTextBox.Text.Trim(),

                ProblemDescription =
                    ProblemDescriptionTextBox.Text.Trim(),

                Priority = priority,

                Status = status,

                CreatedDate = DateTime.Now
            };

            db.Complaints.Add(complaint);
            db.SaveChanges();

            MessageBox.Show(
                "Complaint saved successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to save complaint.\n\n" + ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}