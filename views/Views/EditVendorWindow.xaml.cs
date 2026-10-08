using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class EditVendorWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly int vendorId;

    public EditVendorWindow(int vendorId)
    {
        InitializeComponent();

        this.vendorId = vendorId;

        StatusComboBox.SelectedIndex = 0;

        LoadVendor();
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private void LoadVendor()
    {
        try
        {
            using var db = CreateDbContext();

            var vendor = db.Vendors
                .FirstOrDefault(x => x.VendorId == vendorId);

            if (vendor == null)
            {
                MessageBox.Show(
                    "Vendor not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
                return;
            }

            VendorNameTextBox.Text =
                vendor.VendorName;

            ContactPersonTextBox.Text =
                vendor.ContactPerson ?? string.Empty;

            PhoneTextBox.Text =
                vendor.Phone ?? string.Empty;

            EmailTextBox.Text =
                vendor.Email ?? string.Empty;

            AddressTextBox.Text =
                vendor.Address ?? string.Empty;

            StatusComboBox.SelectedIndex =
                vendor.IsActive ? 0 : 1;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load vendor.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SaveChangesButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string vendorName =
            VendorNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(vendorName))
        {
            MessageBox.Show(
                "Please enter the vendor name.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        string? contactPerson =
            string.IsNullOrWhiteSpace(
                ContactPersonTextBox.Text)
                ? null
                : ContactPersonTextBox.Text.Trim();

        string? phone =
            string.IsNullOrWhiteSpace(
                PhoneTextBox.Text)
                ? null
                : PhoneTextBox.Text.Trim();

        string? email =
            string.IsNullOrWhiteSpace(
                EmailTextBox.Text)
                ? null
                : EmailTextBox.Text.Trim();

        string? address =
            string.IsNullOrWhiteSpace(
                AddressTextBox.Text)
                ? null
                : AddressTextBox.Text.Trim();

        bool isActive = true;

        if (StatusComboBox.SelectedItem
            is ComboBoxItem selectedItem)
        {
            string status =
                selectedItem.Content?.ToString()
                ?? "Active";

            isActive = status == "Active";
        }

        try
        {
            using var db = CreateDbContext();

            var vendor = db.Vendors
                .FirstOrDefault(x =>
                    x.VendorId == vendorId);

            if (vendor == null)
            {
                MessageBox.Show(
                    "Vendor not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            bool duplicateExists = db.Vendors
                .Any(x =>
                    x.VendorId != vendorId &&
                    x.VendorName.ToLower() ==
                    vendorName.ToLower());

            if (duplicateExists)
            {
                MessageBox.Show(
                    "Another vendor with this name already exists.",
                    "Duplicate Vendor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            vendor.VendorName =
                vendorName;

            vendor.ContactPerson =
                contactPerson;

            vendor.Phone =
                phone;

            vendor.Email =
                email;

            vendor.Address =
                address;

            vendor.IsActive =
                isActive;

            db.SaveChanges();

            MessageBox.Show(
                "Vendor updated successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to update vendor.\n\n" +
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