using System;
using System.Linq;
using System.Windows;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class AddAssetWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public AddAssetWindow()
    {
        InitializeComponent();
        LoadDropdowns();
    }

    private AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        return new AppDbContext(options);
    }

    private void LoadDropdowns()
    {
        try
        {
            using var db = CreateDbContext();

            // Asset Types
            AssetTypeComboBox.ItemsSource = db.AssetTypes
                .Where(x => x.IsActive)
                .OrderBy(x => x.AssetTypeName)
                .ToList();

            AssetTypeComboBox.DisplayMemberPath = "AssetTypeName";
            AssetTypeComboBox.SelectedValuePath = "AssetTypeId";

            // Divisions
            DivisionComboBox.ItemsSource = db.Divisions
                .Where(x => x.IsActive)
                .OrderBy(x => x.DivisionName)
                .ToList();

            DivisionComboBox.DisplayMemberPath = "DivisionName";
            DivisionComboBox.SelectedValuePath = "DivisionId";

            // Departments
            DepartmentComboBox.ItemsSource = db.Departments
                .OrderBy(x => x.DepartmentName)
                .ToList();

            DepartmentComboBox.DisplayMemberPath = "DepartmentName";
            DepartmentComboBox.SelectedValuePath = "DepartmentId";

            // Locations
            LocationComboBox.ItemsSource = db.Locations
                .OrderBy(x => x.LocationName)
                .ToList();

            LocationComboBox.DisplayMemberPath = "LocationName";
            LocationComboBox.SelectedValuePath = "LocationId";

            // Employees
            EmployeeComboBox.ItemsSource = db.Employees
                .OrderBy(x => x.EmployeeName)
                .ToList();

            EmployeeComboBox.DisplayMemberPath = "EmployeeName";
            EmployeeComboBox.SelectedValuePath = "EmployeeId";

            // Vendors
            VendorComboBox.ItemsSource = db.Vendors
                .Where(x => x.IsActive)
                .OrderBy(x => x.VendorName)
                .ToList();

            VendorComboBox.DisplayMemberPath = "VendorName";
            VendorComboBox.SelectedValuePath = "VendorId";

            // Default values
            StatusComboBox.SelectedIndex = 0;
            ConditionComboBox.SelectedIndex = 0;

            MessageTextBlock.Text = "";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load dropdown information.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SaveAssetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MessageTextBlock.Text = "";

        try
        {
            // Validate Item Number
            if (string.IsNullOrWhiteSpace(ItemNumberTextBox.Text))
            {
                MessageTextBlock.Text =
                    "Please enter the item number.";

                ItemNumberTextBox.Focus();
                return;
            }

            // Validate Asset Type
            if (AssetTypeComboBox.SelectedValue == null)
            {
                MessageTextBlock.Text =
                    "Please select an asset type.";

                return;
            }

            // Validate Division
            if (DivisionComboBox.SelectedValue == null)
            {
                MessageTextBlock.Text =
                    "Please select a division.";

                return;
            }

            // Purchase cost
            decimal? purchaseCost = null;

            if (!string.IsNullOrWhiteSpace(PurchaseCostTextBox.Text))
            {
                if (!decimal.TryParse(
                        PurchaseCostTextBox.Text,
                        out decimal parsedCost))
                {
                    MessageTextBlock.Text =
                        "Please enter a valid purchase cost.";

                    PurchaseCostTextBox.Focus();
                    return;
                }

                purchaseCost = parsedCost;
            }

            // Expected life
            int? expectedLife = null;

            if (!string.IsNullOrWhiteSpace(
                    ExpectedLifeTextBox.Text))
            {
                if (!int.TryParse(
                        ExpectedLifeTextBox.Text,
                        out int parsedLife) ||
                    parsedLife <= 0)
                {
                    MessageTextBlock.Text =
                        "Please enter a valid expected life.";

                    ExpectedLifeTextBox.Focus();
                    return;
                }

                expectedLife = parsedLife;
            }

            using var db = CreateDbContext();

            // Check duplicate item number
            bool itemExists = db.Assets.Any(
                a => a.ItemNumber ==
                     ItemNumberTextBox.Text.Trim());

            if (itemExists)
            {
                MessageTextBlock.Text =
                    "An asset with this item number already exists.";

                ItemNumberTextBox.Focus();
                return;
            }

            // Create asset
            var asset = new BEMLPropertyManagement.Models.Asset
            {
                ItemNumber = ItemNumberTextBox.Text.Trim(),

                AssetTypeId =
                    (int)AssetTypeComboBox.SelectedValue,

                Brand = string.IsNullOrWhiteSpace(
                    BrandTextBox.Text)
                    ? null
                    : BrandTextBox.Text.Trim(),

                Model = string.IsNullOrWhiteSpace(
                    ModelTextBox.Text)
                    ? null
                    : ModelTextBox.Text.Trim(),

                SerialNumber = string.IsNullOrWhiteSpace(
                    SerialNumberTextBox.Text)
                    ? null
                    : SerialNumberTextBox.Text.Trim(),

                DivisionId =
                    (int)DivisionComboBox.SelectedValue,

                DepartmentId =
                    DepartmentComboBox.SelectedValue == null
                    ? null
                    : (int?)DepartmentComboBox.SelectedValue,

                LocationId =
                    LocationComboBox.SelectedValue == null
                    ? null
                    : (int?)LocationComboBox.SelectedValue,

                EmployeeId =
                    EmployeeComboBox.SelectedValue == null
                    ? null
                    : (int?)EmployeeComboBox.SelectedValue,

                VendorId =
                    VendorComboBox.SelectedValue == null
                    ? null
                    : (int?)VendorComboBox.SelectedValue,

                PurchaseDate =
                    PurchaseDatePicker.SelectedDate,

                PurchaseCost = purchaseCost,

                WarrantyStartDate =
                    WarrantyStartDatePicker.SelectedDate,

                WarrantyEndDate =
                    WarrantyEndDatePicker.SelectedDate,

                ExpectedLifeYears = expectedLife,

                ExpectedReplacementDate =
                    ReplacementDatePicker.SelectedDate,

                Status =
                    (StatusComboBox.SelectedItem
                        as System.Windows.Controls.ComboBoxItem)
                    ?.Content?.ToString()
                    ?? "Active",

                AssetCondition =
                    (ConditionComboBox.SelectedItem
                        as System.Windows.Controls.ComboBoxItem)
                    ?.Content?.ToString()
                    ?? "Good",

                Remarks = string.IsNullOrWhiteSpace(
                    RemarksTextBox.Text)
                    ? null
                    : RemarksTextBox.Text.Trim(),

                CreatedDate = DateTime.Now
            };

            db.Assets.Add(asset);

            db.SaveChanges();

            MessageBox.Show(
                "Asset added successfully.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to save the asset.\n\n" +
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