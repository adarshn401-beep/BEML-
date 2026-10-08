using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BEMLPropertyManagement.Data;
using BEMLPropertyManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class AddServiceRecordWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public AddServiceRecordWindow()
    {
        InitializeComponent();

        LoadAssets();

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

    private void SaveServiceButton_Click(object sender, RoutedEventArgs e)
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

            if (string.IsNullOrWhiteSpace(ProblemDescriptionTextBox.Text))
            {
                MessageBox.Show(
                    "Please enter Problem Description.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            decimal? serviceCost = null;

            if (!string.IsNullOrWhiteSpace(ServiceCostTextBox.Text))
            {
                if (!decimal.TryParse(
                        ServiceCostTextBox.Text.Trim(),
                        out decimal parsedCost))
                {
                    MessageBox.Show(
                        "Please enter a valid Service Cost.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                serviceCost = parsedCost;
            }

            using var db = CreateDbContext();

            var serviceRecord = new ServiceRecord
            {
                AssetId = (int)AssetComboBox.SelectedValue,

                ComplaintDate = ComplaintDatePicker.SelectedDate.Value,

                ProblemDescription =
                    ProblemDescriptionTextBox.Text.Trim(),

                ServiceDate = ServiceDatePicker.SelectedDate,

                ServiceProvider =
                    string.IsNullOrWhiteSpace(ServiceProviderTextBox.Text)
                        ? null
                        : ServiceProviderTextBox.Text.Trim(),

                WorkDescription =
                    string.IsNullOrWhiteSpace(WorkDescriptionTextBox.Text)
                        ? null
                        : WorkDescriptionTextBox.Text.Trim(),

                ServiceCost = serviceCost,

                Status = StatusComboBox.SelectedItem is ComboBoxItem statusItem
                    ? statusItem.Content?.ToString() ?? "Open"
                    : "Open",

                NextServiceDate =
                    NextServiceDatePicker.SelectedDate,

                CreatedDate = DateTime.Now
            };

            db.ServiceRecords.Add(serviceRecord);

            db.SaveChanges();

            MessageBox.Show(
                "Service record saved successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to save service record.\n\n" + ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}