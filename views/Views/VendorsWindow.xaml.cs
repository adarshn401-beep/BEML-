using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class VendorsWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public VendorsWindow()
    {
        InitializeComponent();
        LoadVendors();
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private void LoadVendors()
    {
        try
        {
            using var db = CreateDbContext();

            var vendors = db.Vendors
                .OrderBy(x => x.VendorName)
                .Select(x => new VendorDisplay
                {
                    VendorId = x.VendorId,
                    VendorName = x.VendorName,
                    ContactPerson = x.ContactPerson ?? "-",
                    Phone = x.Phone ?? "-",
                    Email = x.Email ?? "-",
                    Address = x.Address ?? "-",
                    Status = x.IsActive ? "Active" : "Inactive"
                })
                .ToList();

            VendorDataGrid.ItemsSource = vendors;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load vendors.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddVendorButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var addVendorWindow = new AddVendorWindow();

        addVendorWindow.ShowDialog();

        LoadVendors();
    }

    private void ViewVendorButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not VendorDisplay selectedVendor)
            return;

        MessageBox.Show(
            $"Vendor Details\n\n" +
            $"Vendor Name: {selectedVendor.VendorName}\n" +
            $"Contact Person: {selectedVendor.ContactPerson}\n" +
            $"Phone: {selectedVendor.Phone}\n" +
            $"Email: {selectedVendor.Email}\n" +
            $"Address: {selectedVendor.Address}\n" +
            $"Status: {selectedVendor.Status}",
            "Vendor Details",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void EditVendorButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not VendorDisplay selectedVendor)
            return;

        var editVendorWindow =
            new EditVendorWindow(
                selectedVendor.VendorId);

        editVendorWindow.ShowDialog();

        LoadVendors();
    }

    private void DeleteVendorButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not VendorDisplay selectedVendor)
            return;

        MessageBoxResult result = MessageBox.Show(
            $"Are you sure you want to delete this vendor?\n\n" +
            $"Vendor: {selectedVendor.VendorName}",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = CreateDbContext();

            var vendor = db.Vendors
                .FirstOrDefault(x =>
                    x.VendorId == selectedVendor.VendorId);

            if (vendor == null)
            {
                MessageBox.Show(
                    "Vendor not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                LoadVendors();
                return;
            }

            db.Vendors.Remove(vendor);
            db.SaveChanges();

            MessageBox.Show(
                "Vendor deleted successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadVendors();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to delete vendor.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SearchButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        SearchVendors();
    }

    private void SearchTextBox_KeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchVendors();
        }
    }

    private void SearchVendors()
    {
        try
        {
            string searchText = SearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText) ||
                searchText == "Search vendor...")
            {
                LoadVendors();
                return;
            }

            using var db = CreateDbContext();

            var vendors = db.Vendors
                .Where(x =>
                    x.VendorName.Contains(searchText) ||
                    (x.ContactPerson != null &&
                     x.ContactPerson.Contains(searchText)) ||
                    (x.Phone != null &&
                     x.Phone.Contains(searchText)) ||
                    (x.Email != null &&
                     x.Email.Contains(searchText)) ||
                    (x.Address != null &&
                     x.Address.Contains(searchText)))
                .OrderBy(x => x.VendorName)
                .Select(x => new VendorDisplay
                {
                    VendorId = x.VendorId,
                    VendorName = x.VendorName,
                    ContactPerson = x.ContactPerson ?? "-",
                    Phone = x.Phone ?? "-",
                    Email = x.Email ?? "-",
                    Address = x.Address ?? "-",
                    Status = x.IsActive ? "Active" : "Inactive"
                })
                .ToList();

            VendorDataGrid.ItemsSource = vendors;

            if (vendors.Count == 0)
            {
                MessageBox.Show(
                    "No vendors found.",
                    "Search",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to search vendors.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private class VendorDisplay
    {
        public int VendorId { get; set; }

        public string VendorName { get; set; } = string.Empty;

        public string ContactPerson { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}