using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class ReplacementWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public ReplacementWindow()
    {
        InitializeComponent();
        LoadReplacementRecords();
    }

    // =========================
    // DATABASE
    // =========================

    private AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        return new AppDbContext(options);
    }

    // =========================
    // LOAD RECORDS
    // =========================

    private void LoadReplacementRecords()
    {
        try
        {
            using var db = CreateDbContext();

            var replacements =
                (
                    from replacement in db.ReplacementRecords

                    join oldAsset in db.Assets
                        on replacement.OldAssetId equals oldAsset.AssetId

                    join newAsset in db.Assets
                        on replacement.NewAssetId equals newAsset.AssetId
                        into newAssetGroup

                    from newAsset in newAssetGroup.DefaultIfEmpty()

                    select new ReplacementDisplayRow
                    {
                        ReplacementId =
                            replacement.ReplacementId,

                        OldItemNumber =
                            oldAsset.ItemNumber,

                        NewItemNumber =
                            newAsset != null
                                ? newAsset.ItemNumber
                                : "-",

                        ReplacementDate =
                            replacement.ReplacementDate,

                        ReplacementReason =
                            replacement.ReplacementReason,

                        OldAssetCondition =
                            replacement.OldAssetCondition ?? "-",

                        DisposalStatus =
                            replacement.OldAssetDisposalStatus ?? "-",

                        ApprovedBy =
                            replacement.ApprovedBy ?? "-",

                        ReplacementCost =
                            replacement.ReplacementCost,

                        Remarks =
                            replacement.Remarks ?? "-"
                    }
                )
                .OrderByDescending(
                    x => x.ReplacementDate)
                .ToList();

            ReplacementDataGrid.ItemsSource =
                replacements;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load replacement records.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // =========================
    // ADD
    // =========================

    private void AddReplacementButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var addWindow =
            new AddReplacementWindow();

        addWindow.Owner = this;

        bool? result =
            addWindow.ShowDialog();

        if (result == true)
        {
            LoadReplacementRecords();
        }
    }

    // =========================
    // SEARCH BUTTON
    // =========================

    private void SearchButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        SearchReplacementRecords();
    }

    // =========================
    // SEARCH ENTER KEY
    // =========================

    private void SearchTextBox_KeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchReplacementRecords();
        }
    }

    // =========================
    // SEARCH
    // =========================

    private void SearchReplacementRecords()
    {
        try
        {
            string searchText =
                SearchTextBox.Text.Trim();

            if (searchText ==
                "Search replacement...")
            {
                searchText = string.Empty;
            }

            using var db = CreateDbContext();

            var replacements =
                (
                    from replacement in db.ReplacementRecords

                    join oldAsset in db.Assets
                        on replacement.OldAssetId
                            equals oldAsset.AssetId

                    join newAsset in db.Assets
                        on replacement.NewAssetId
                            equals newAsset.AssetId
                        into newAssetGroup

                    from newAsset in
                        newAssetGroup.DefaultIfEmpty()

                    select new ReplacementDisplayRow
                    {
                        ReplacementId =
                            replacement.ReplacementId,

                        OldItemNumber =
                            oldAsset.ItemNumber,

                        NewItemNumber =
                            newAsset != null
                                ? newAsset.ItemNumber
                                : "-",

                        ReplacementDate =
                            replacement.ReplacementDate,

                        ReplacementReason =
                            replacement.ReplacementReason,

                        OldAssetCondition =
                            replacement.OldAssetCondition ?? "-",

                        DisposalStatus =
                            replacement.OldAssetDisposalStatus ?? "-",

                        ApprovedBy =
                            replacement.ApprovedBy ?? "-",

                        ReplacementCost =
                            replacement.ReplacementCost,

                        Remarks =
                            replacement.Remarks ?? "-"
                    }
                )
                .Where(x =>
                    string.IsNullOrEmpty(searchText) ||

                    x.OldItemNumber.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase)

                    ||

                    x.NewItemNumber.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase)

                    ||

                    x.ReplacementReason.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase)

                    ||

                    x.OldAssetCondition.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase)

                    ||

                    x.DisposalStatus.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase)

                    ||

                    x.ApprovedBy.Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase)
                )
                .OrderByDescending(
                    x => x.ReplacementDate)
                .ToList();

            ReplacementDataGrid.ItemsSource =
                replacements;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to search replacement records.\n\n" +
                ex.Message,
                "Search Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // =========================
    // VIEW
    // =========================

    private void ViewReplacementButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.DataContext
                is not ReplacementDisplayRow selected)
        {
            return;
        }

        MessageBox.Show(
            "Replacement Details\n\n" +

            "Old Asset: " +
            selected.OldItemNumber +

            "\nNew Asset: " +
            selected.NewItemNumber +

            "\nReplacement Date: " +
            selected.ReplacementDate.ToString("dd-MM-yyyy") +

            "\nReason: " +
            selected.ReplacementReason +

            "\nOld Condition: " +
            selected.OldAssetCondition +

            "\nDisposal Status: " +
            selected.DisposalStatus +

            "\nApproved By: " +
            selected.ApprovedBy +

            "\nReplacement Cost: " +
            (selected.ReplacementCost.HasValue
                ? selected.ReplacementCost.Value.ToString("0.00")
                : "-") +

            "\nRemarks: " +
            selected.Remarks,

            "Replacement Details",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    // =========================
    // EDIT
    // =========================

    private void EditReplacementButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.DataContext
                is not ReplacementDisplayRow selected)
        {
            return;
        }

        var editWindow =
            new EditReplacementWindow(
                selected.ReplacementId);

        editWindow.Owner = this;

        bool? result =
            editWindow.ShowDialog();

        if (result == true)
        {
            LoadReplacementRecords();
        }
    }

    // =========================
    // DELETE
    // =========================

    private void DeleteReplacementButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.DataContext
                is not ReplacementDisplayRow selected)
        {
            return;
        }

        MessageBoxResult confirmation =
            MessageBox.Show(
                "Are you sure you want to delete this replacement record?\n\n" +

                "Old Asset: " +
                selected.OldItemNumber +

                "\nNew Asset: " +
                selected.NewItemNumber,

                "Confirm Delete",

                MessageBoxButton.YesNo,

                MessageBoxImage.Warning);

        if (confirmation !=
            MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            using var db = CreateDbContext();

            var replacement =
                db.ReplacementRecords
                    .FirstOrDefault(
                        x =>
                            x.ReplacementId ==
                            selected.ReplacementId);

            if (replacement == null)
            {
                MessageBox.Show(
                    "Replacement record was not found.",
                    "Delete Replacement",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            db.ReplacementRecords.Remove(
                replacement);

            db.SaveChanges();

            MessageBox.Show(
                "Replacement record deleted successfully.",
                "Delete Replacement",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadReplacementRecords();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to delete replacement record.\n\n" +
                ex.Message,
                "Delete Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // =========================
    // DISPLAY MODEL
    // =========================

    private class ReplacementDisplayRow
    {
        public int ReplacementId { get; set; }

        public string OldItemNumber { get; set; } =
            string.Empty;

        public string NewItemNumber { get; set; } =
            string.Empty;

        public DateTime ReplacementDate { get; set; }

        public string ReplacementReason { get; set; } =
            string.Empty;

        public string OldAssetCondition { get; set; } =
            string.Empty;

        public string DisposalStatus { get; set; } =
            string.Empty;

        public string ApprovedBy { get; set; } =
            string.Empty;

        public decimal? ReplacementCost { get; set; }

        public string Remarks { get; set; } =
            string.Empty;
    }
}