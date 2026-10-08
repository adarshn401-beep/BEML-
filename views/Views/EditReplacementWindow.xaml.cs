using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BEMLPropertyManagement.Data;
using BEMLPropertyManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class EditReplacementWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly int replacementId;

    public EditReplacementWindow(int replacementId)
    {
        InitializeComponent();

        this.replacementId = replacementId;

        LoadAssets();
        LoadReplacement();
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

    private void LoadReplacement()
    {
        try
        {
            using var db = CreateDbContext();

            var replacement = db.ReplacementRecords
                .FirstOrDefault(x =>
                    x.ReplacementId == replacementId);

            if (replacement == null)
            {
                MessageBox.Show(
                    "Replacement record not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
                return;
            }

            OldAssetComboBox.SelectedValue =
                replacement.OldAssetId;

            NewAssetComboBox.SelectedValue =
                replacement.NewAssetId;

            ReplacementDatePicker.SelectedDate =
                replacement.ReplacementDate;

            ReplacementReasonTextBox.Text =
                replacement.ReplacementReason;

            SelectComboBoxItem(
                OldAssetConditionComboBox,
                replacement.OldAssetCondition);

            SelectComboBoxItem(
                DisposalStatusComboBox,
                replacement.OldAssetDisposalStatus);

            ApprovedByTextBox.Text =
                replacement.ApprovedBy ?? string.Empty;

            ReplacementCostTextBox.Text =
                replacement.ReplacementCost?.ToString() ?? string.Empty;

            RemarksTextBox.Text =
                replacement.Remarks ?? string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load replacement record.\n\n" +
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
            return;

        foreach (var item in comboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem &&
                comboBoxItem.Content?.ToString() == value)
            {
                comboBox.SelectedItem = comboBoxItem;
                return;
            }
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

    private void SaveChangesButton_Click(
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

            var replacement = db.ReplacementRecords
                .FirstOrDefault(x =>
                    x.ReplacementId == replacementId);

            if (replacement == null)
            {
                MessageBox.Show(
                    "Replacement record not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            replacement.OldAssetId =
                oldAssetId;

            replacement.NewAssetId =
                newAssetId;

            replacement.ReplacementDate =
                ReplacementDatePicker.SelectedDate.Value;

            replacement.ReplacementReason =
                ReplacementReasonTextBox.Text.Trim();

            replacement.OldAssetCondition =
                GetComboBoxValue(
                    OldAssetConditionComboBox);

            replacement.OldAssetDisposalStatus =
                GetComboBoxValue(
                    DisposalStatusComboBox);

            replacement.ApprovedBy =
                string.IsNullOrWhiteSpace(
                    ApprovedByTextBox.Text)
                    ? null
                    : ApprovedByTextBox.Text.Trim();

            replacement.ReplacementCost =
                replacementCost;

            replacement.Remarks =
                string.IsNullOrWhiteSpace(
                    RemarksTextBox.Text)
                    ? null
                    : RemarksTextBox.Text.Trim();

            db.SaveChanges();

            MessageBox.Show(
                "Replacement record updated successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to update replacement record.\n\n" +
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