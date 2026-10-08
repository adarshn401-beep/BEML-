using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class AddReplacementWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public AddReplacementWindow()
    {
        InitializeComponent();

        LoadAssets();

        ReplacementDatePicker.SelectedDate = DateTime.Today;

        OldAssetConditionComboBox.SelectedIndex = 0;
        DisposalStatusComboBox.SelectedIndex = 0;
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

            OldAssetComboBox.ItemsSource = assets;
            OldAssetComboBox.DisplayMemberPath = "ItemNumber";
            OldAssetComboBox.SelectedValuePath = "AssetId";

            NewAssetComboBox.ItemsSource = assets.ToList();
            NewAssetComboBox.DisplayMemberPath = "ItemNumber";
            NewAssetComboBox.SelectedValuePath = "AssetId";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load assets.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SaveReplacementButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (OldAssetComboBox.SelectedValue == null)
        {
            MessageBox.Show(
                "Please select the old asset.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (NewAssetComboBox.SelectedValue == null)
        {
            MessageBox.Show(
                "Please select the new asset.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        int oldAssetId =
            (int)OldAssetComboBox.SelectedValue;

        int newAssetId =
            (int)NewAssetComboBox.SelectedValue;

        if (oldAssetId == newAssetId)
        {
            MessageBox.Show(
                "Old Asset and New Asset cannot be the same.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (!ReplacementDatePicker.SelectedDate.HasValue)
        {
            MessageBox.Show(
                "Please select the replacement date.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (string.IsNullOrWhiteSpace(
            ReplacementReasonTextBox.Text))
        {
            MessageBox.Show(
                "Please enter the replacement reason.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        decimal? replacementCost = null;

        if (!string.IsNullOrWhiteSpace(
            ReplacementCostTextBox.Text))
        {
            if (!decimal.TryParse(
                ReplacementCostTextBox.Text.Trim(),
                out decimal parsedCost))
            {
                MessageBox.Show(
                    "Please enter a valid replacement cost.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (parsedCost < 0)
            {
                MessageBox.Show(
                    "Replacement cost cannot be negative.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            replacementCost = parsedCost;
        }

        try
        {
            using var db = CreateDbContext();

            var replacement = new Models.ReplacementRecord
            {
                OldAssetId = oldAssetId,

                NewAssetId = newAssetId,

                ReplacementDate =
                    ReplacementDatePicker.SelectedDate.Value,

                ReplacementReason =
                    ReplacementReasonTextBox.Text.Trim(),

                OldAssetCondition =
                    GetComboBoxValue(
                        OldAssetConditionComboBox),

                OldAssetDisposalStatus =
                    GetComboBoxValue(
                        DisposalStatusComboBox),

                ApprovedBy =
                    string.IsNullOrWhiteSpace(
                        ApprovedByTextBox.Text)
                        ? null
                        : ApprovedByTextBox.Text.Trim(),

                ReplacementCost = replacementCost,

                Remarks =
                    string.IsNullOrWhiteSpace(
                        RemarksTextBox.Text)
                        ? null
                        : RemarksTextBox.Text.Trim()
            };

            db.ReplacementRecords.Add(replacement);
            db.SaveChanges();

            MessageBox.Show(
                "Replacement record saved successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to save replacement record.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private string? GetComboBoxValue(
        ComboBox comboBox)
    {
        if (comboBox.SelectedItem is ComboBoxItem item)
        {
            return item.Content?.ToString();
        }

        return null;
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}