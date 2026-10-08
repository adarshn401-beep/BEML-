using System;
using System.Windows;
using System.Windows.Controls;
using BEMLPropertyManagement.Data;
using BEMLPropertyManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class AddVendorWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public AddVendorWindow()
    {
        InitializeComponent();

        StatusComboBox.SelectedIndex = 0;
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private void SaveVendorButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string vendorName = VendorNameTextBox.Text.Trim();

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
            string.IsNullOrWhiteSpace(ContactPersonTextBox.Text)
                ? null
                : ContactPersonTextBox.Text.Trim();

        string? phone =
            string.IsNullOrWhiteSpace(PhoneTextBox.Text)
                ? null
                : PhoneTextBox.Text.Trim();

        string? email =
            string.IsNullOrWhiteSpace(EmailTextBox.Text)
                ? null
                : EmailTextBox.Text.Trim();

        string? address =
            string.IsNullOrWhiteSpace(AddressTextBox.Text)
                ? null
                : AddressTextBox.Text.Trim();

        string status = "Active";

        if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            status = selectedItem.Content?.ToString() ?? "Active";
        }

        try
        {
            using var db = CreateDbContext();

            var existingVendor = db.Vendors
                .FirstOrDefault(x =>
                    x.VendorName.ToLower() ==
                    vendorName.ToLower());

            if (existingVendor != null)
            {
                MessageBox.Show(
                    "A vendor with this name already exists.",
                    "Duplicate Vendor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var vendor = new Vendor
            {
                VendorName = vendorName,
                ContactPerson = contactPerson,
                Phone = phone,
                Email = email,
                Address = address,
                IsActive = status == "Active"
            };

            db.Vendors.Add(vendor);
            db.SaveChanges();

            MessageBox.Show(
                "Vendor saved successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to save vendor.\n\n" +
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