using System;
using System.Linq;
using System.Windows;
using BEMLPropertyManagement.Data;
using BEMLPropertyManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class EditAssetWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly int assetId;

    public EditAssetWindow(int selectedAssetId)
    {
        InitializeComponent();

        assetId = selectedAssetId;

        LoadDropdowns();
        LoadAsset();
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

            AssetTypeComboBox.ItemsSource =
                db.AssetTypes
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.AssetTypeName)
                    .ToList();

            DivisionComboBox.ItemsSource =
                db.Divisions
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.DivisionName)
                    .ToList();

            DepartmentComboBox.ItemsSource =
                db.Departments
                    .OrderBy(x => x.DepartmentName)
                    .ToList();

            LocationComboBox.ItemsSource =
                db.Locations
                    .OrderBy(x => x.LocationName)
                    .ToList();

            EmployeeComboBox.ItemsSource =
                db.Employees
                    .OrderBy(x => x.EmployeeName)
                    .ToList();

            VendorComboBox.ItemsSource =
                db.Vendors
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.VendorName)
                    .ToList();
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

    private void LoadAsset()
    {
        try
        {
            using var db = CreateDbContext();

            var asset = db.Assets
                .FirstOrDefault(x => x.AssetId == assetId);

            if (asset == null)
            {
                MessageBox.Show(
                    "The selected asset could not be found.",
                    "Asset Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Close();
                return;
            }

            ItemNumberTextBox.Text = asset.ItemNumber;
            BrandTextBox.Text = asset.Brand ?? string.Empty;
            ModelTextBox.Text = asset.Model ?? string.Empty;
            SerialNumberTextBox.Text = asset.SerialNumber ?? string.Empty;

            AssetTypeComboBox.SelectedValue = asset.AssetTypeId;
            DivisionComboBox.SelectedValue = asset.DivisionId;

            if (asset.DepartmentId.HasValue)
                DepartmentComboBox.SelectedValue = asset.DepartmentId.Value;

            if (asset.LocationId.HasValue)
                LocationComboBox.SelectedValue = asset.LocationId.Value;

            if (asset.EmployeeId.HasValue)
                EmployeeComboBox.SelectedValue = asset.EmployeeId.Value;

            if (asset.VendorId.HasValue)
                VendorComboBox.SelectedValue = asset.VendorId.Value;

            PurchaseDatePicker.SelectedDate = asset.PurchaseDate;
            PurchaseCostTextBox.Text =
                asset.PurchaseCost?.ToString("0.00") ?? string.Empty;

            WarrantyStartDatePicker.SelectedDate =
                asset.WarrantyStartDate;

            WarrantyEndDatePicker.SelectedDate =
                asset.WarrantyEndDate;

            ExpectedLifeTextBox.Text =
                asset.ExpectedLifeYears?.ToString() ?? string.Empty;

            ReplacementDatePicker.SelectedDate =
                asset.ExpectedReplacementDate;

            SelectComboBoxItem(StatusComboBox, asset.Status);
            SelectComboBoxItem(ConditionComboBox, asset.AssetCondition);

            RemarksTextBox.Text =
                asset.Remarks ?? string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load asset information.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SelectComboBoxItem(
        System.Windows.Controls.ComboBox comboBox,
        string value)
    {
        foreach (var item in comboBox.Items)
        {
            if (item is System.Windows.Controls.ComboBoxItem comboBoxItem &&
                string.Equals(
                    comboBoxItem.Content?.ToString(),
                    value,
                    StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedItem = comboBoxItem;
                return;
            }
        }

        comboBox.SelectedIndex = 0;
    }

    private void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            if (AssetTypeComboBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Please select an asset type.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (DivisionComboBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Please select a division.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            decimal? purchaseCost = null;

            if (!string.IsNullOrWhiteSpace(PurchaseCostTextBox.Text))
            {
                if (!decimal.TryParse(
                        PurchaseCostTextBox.Text.Trim(),
                        out decimal parsedCost))
                {
                    MessageBox.Show(
                        "Please enter a valid purchase cost.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                purchaseCost = parsedCost;
            }

            int? expectedLife = null;

            if (!string.IsNullOrWhiteSpace(ExpectedLifeTextBox.Text))
            {
                if (!int.TryParse(
                        ExpectedLifeTextBox.Text.Trim(),
                        out int parsedLife) ||
                    parsedLife < 0)
                {
                    MessageBox.Show(
                        "Please enter a valid expected life in years.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                expectedLife = parsedLife;
            }

            using var db = CreateDbContext();

            var asset = db.Assets
                .FirstOrDefault(x => x.AssetId == assetId);

            if (asset == null)
            {
                MessageBox.Show(
                    "The selected asset could not be found.",
                    "Asset Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            asset.AssetTypeId =
                Convert.ToInt32(AssetTypeComboBox.SelectedValue);

            asset.Brand =
                string.IsNullOrWhiteSpace(BrandTextBox.Text)
                    ? null
                    : BrandTextBox.Text.Trim();

            asset.Model =
                string.IsNullOrWhiteSpace(ModelTextBox.Text)
                    ? null
                    : ModelTextBox.Text.Trim();

            asset.SerialNumber =
                string.IsNullOrWhiteSpace(SerialNumberTextBox.Text)
                    ? null
                    : SerialNumberTextBox.Text.Trim();

            asset.DivisionId =
                Convert.ToInt32(DivisionComboBox.SelectedValue);

            asset.DepartmentId =
                DepartmentComboBox.SelectedValue != null
                    ? Convert.ToInt32(DepartmentComboBox.SelectedValue)
                    : null;

            asset.LocationId =
                LocationComboBox.SelectedValue != null
                    ? Convert.ToInt32(LocationComboBox.SelectedValue)
                    : null;

            asset.EmployeeId =
                EmployeeComboBox.SelectedValue != null
                    ? Convert.ToInt32(EmployeeComboBox.SelectedValue)
                    : null;

            asset.VendorId =
                VendorComboBox.SelectedValue != null
                    ? Convert.ToInt32(VendorComboBox.SelectedValue)
                    : null;

            asset.PurchaseDate =
                PurchaseDatePicker.SelectedDate;

            asset.PurchaseCost =
                purchaseCost;

            asset.WarrantyStartDate =
                WarrantyStartDatePicker.SelectedDate;

            asset.WarrantyEndDate =
                WarrantyEndDatePicker.SelectedDate;

            asset.ExpectedLifeYears =
                expectedLife;

            asset.ExpectedReplacementDate =
                ReplacementDatePicker.SelectedDate;

            if (StatusComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem statusItem)
            {
                asset.Status =
                    statusItem.Content?.ToString() ?? "Active";
            }

            if (ConditionComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem conditionItem)
            {
                asset.AssetCondition =
                    conditionItem.Content?.ToString() ?? "Good";
            }

            asset.Remarks =
                string.IsNullOrWhiteSpace(RemarksTextBox.Text)
                    ? null
                    : RemarksTextBox.Text.Trim();

            asset.UpdatedDate = DateTime.Now;

            db.SaveChanges();

            MessageBox.Show(
                "Asset details updated successfully.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to save asset changes.\n\n" +
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
        DialogResult = false;
        Close();
    }
}