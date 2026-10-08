using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BEMLPropertyManagement.Data;
using BEMLPropertyManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class EditWarrantyWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly int warrantyId;

    public EditWarrantyWindow(int warrantyId)
    {
        InitializeComponent();

        this.warrantyId = warrantyId;

        LoadAssets();
        LoadWarranty();
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

    private void LoadWarranty()
    {
        try
        {
            using var db = CreateDbContext();

            var warranty = db.WarrantyRecords
                .FirstOrDefault(x => x.WarrantyId == warrantyId);

            if (warranty == null)
            {
                MessageBox.Show(
                    "Warranty record not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
                return;
            }

            AssetComboBox.SelectedValue = warranty.AssetId;

            WarrantyProviderTextBox.Text =
                warranty.WarrantyProvider ?? string.Empty;

            WarrantyStartDatePicker.SelectedDate =
                warranty.WarrantyStartDate;

            WarrantyEndDatePicker.SelectedDate =
                warranty.WarrantyEndDate;

            SelectComboBoxItem(
                WarrantyTypeComboBox,
                warranty.WarrantyType);

            ExtendedWarrantyCheckBox.IsChecked =
                warranty.IsExtendedWarranty;

            SelectComboBoxItem(
                ClaimStatusComboBox,
                warranty.ClaimStatus);

            ClaimDescriptionTextBox.Text =
                warranty.ClaimDescription ?? string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load warranty record.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SelectComboBoxItem(
        ComboBox comboBox,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            comboBox.SelectedIndex = -1;
            return;
        }

        foreach (ComboBoxItem item in comboBox.Items)
        {
            if (item.Content?.ToString() == value)
            {
                comboBox.SelectedItem = item;
                return;
            }
        }

        comboBox.SelectedIndex = -1;
    }

    private void SaveChangesButton_Click(
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

            var warranty = db.WarrantyRecords
                .FirstOrDefault(x => x.WarrantyId == warrantyId);

            if (warranty == null)
            {
                MessageBox.Show(
                    "Warranty record not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            warranty.AssetId =
                (int)AssetComboBox.SelectedValue;

            warranty.WarrantyProvider =
                string.IsNullOrWhiteSpace(
                    WarrantyProviderTextBox.Text)
                    ? null
                    : WarrantyProviderTextBox.Text.Trim();

            warranty.WarrantyStartDate = startDate;
            warranty.WarrantyEndDate = endDate;

            warranty.WarrantyType =
                WarrantyTypeComboBox.SelectedItem
                    is ComboBoxItem typeItem
                    ? typeItem.Content?.ToString()
                    : null;

            warranty.IsExtendedWarranty =
                ExtendedWarrantyCheckBox.IsChecked == true;

            warranty.ClaimStatus =
                ClaimStatusComboBox.SelectedItem
                    is ComboBoxItem statusItem
                    ? statusItem.Content?.ToString()
                    : null;

            warranty.ClaimDescription =
                string.IsNullOrWhiteSpace(
                    ClaimDescriptionTextBox.Text)
                    ? null
                    : ClaimDescriptionTextBox.Text.Trim();

            db.SaveChanges();

            MessageBox.Show(
                "Warranty record updated successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to update warranty record.\n\n" +
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