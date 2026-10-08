using System;
using System.Linq;
using System.Windows;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class AddWarrantyWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public AddWarrantyWindow()
    {
        InitializeComponent();

        LoadAssets();

        WarrantyStartDatePicker.SelectedDate = DateTime.Today;
        ClaimStatusComboBox.SelectedIndex = 0;
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

            var assets = db.Assets
                .OrderBy(x => x.ItemNumber)
                .ToList();

            AssetComboBox.ItemsSource = assets;
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

    private void SaveWarrantyButton_Click(
        object sender,
        RoutedEventArgs e)
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

        if (!WarrantyStartDatePicker.SelectedDate.HasValue)
        {
            MessageBox.Show(
                "Please select the warranty start date.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (!WarrantyEndDatePicker.SelectedDate.HasValue)
        {
            MessageBox.Show(
                "Please select the warranty end date.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        DateTime startDate =
            WarrantyStartDatePicker.SelectedDate.Value;

        DateTime endDate =
            WarrantyEndDatePicker.SelectedDate.Value;

        if (endDate < startDate)
        {
            MessageBox.Show(
                "Warranty end date cannot be earlier than the start date.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        try
        {
            using var db = CreateDbContext();

            var warranty = new Models.WarrantyRecord
            {
                AssetId = (int)AssetComboBox.SelectedValue,
                WarrantyProvider =
                    string.IsNullOrWhiteSpace(
                        WarrantyProviderTextBox.Text)
                        ? null
                        : WarrantyProviderTextBox.Text.Trim(),

                WarrantyStartDate = startDate,
                WarrantyEndDate = endDate,

                WarrantyType =
                    WarrantyTypeComboBox.SelectedItem
                        is System.Windows.Controls.ComboBoxItem typeItem
                        ? typeItem.Content?.ToString()
                        : null,

                IsExtendedWarranty =
                    ExtendedWarrantyCheckBox.IsChecked == true,

                ClaimStatus =
                    ClaimStatusComboBox.SelectedItem
                        is System.Windows.Controls.ComboBoxItem statusItem
                        ? statusItem.Content?.ToString()
                        : null,

                ClaimDescription =
                    string.IsNullOrWhiteSpace(
                        ClaimDescriptionTextBox.Text)
                        ? null
                        : ClaimDescriptionTextBox.Text.Trim()
            };

            db.WarrantyRecords.Add(warranty);
            db.SaveChanges();

            MessageBox.Show(
                "Warranty record saved successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to save warranty record.\n\n" +
                ex.Message,
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