using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class EditServiceRecordWindow : Window
{
    private readonly int _serviceRecordId;

    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public EditServiceRecordWindow(int serviceRecordId)
    {
        InitializeComponent();

        _serviceRecordId = serviceRecordId;

        LoadAssets();
        LoadServiceRecord();
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

            AssetComboBox.ItemsSource = db.Assets
                .OrderBy(x => x.ItemNumber)
                .ToList();

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

    private void LoadServiceRecord()
    {
        try
        {
            using var db = CreateDbContext();

            var serviceRecord = db.ServiceRecords
                .FirstOrDefault(x => x.ServiceRecordId == _serviceRecordId);

            if (serviceRecord == null)
            {
                MessageBox.Show(
                    "Service record not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
                return;
            }

            AssetComboBox.SelectedValue = serviceRecord.AssetId;

            ComplaintDatePicker.SelectedDate =
                serviceRecord.ComplaintDate;

            ProblemDescriptionTextBox.Text =
                serviceRecord.ProblemDescription;

            ServiceDatePicker.SelectedDate =
                serviceRecord.ServiceDate;

            ServiceProviderTextBox.Text =
                serviceRecord.ServiceProvider ?? string.Empty;

            ServiceCostTextBox.Text =
                serviceRecord.ServiceCost.HasValue
                    ? serviceRecord.ServiceCost.Value.ToString("0.00")
                    : string.Empty;

            WorkDescriptionTextBox.Text =
                serviceRecord.WorkDescription ?? string.Empty;

            SelectStatus(serviceRecord.Status);

            NextServiceDatePicker.SelectedDate =
                serviceRecord.NextServiceDate;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load service record.\n\n" + ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SelectStatus(string status)
    {
        foreach (var item in StatusComboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem &&
                comboBoxItem.Content?.ToString() == status)
            {
                StatusComboBox.SelectedItem = comboBoxItem;
                return;
            }
        }

        StatusComboBox.SelectedIndex = 0;
    }

    private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
    {
        try
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

            if (ComplaintDatePicker.SelectedDate == null)
            {
                MessageBox.Show(
                    "Please select Complaint Date.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(
                    ProblemDescriptionTextBox.Text))
            {
                MessageBox.Show(
                    "Please enter Problem Description.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            decimal? serviceCost = null;

            if (!string.IsNullOrWhiteSpace(
                    ServiceCostTextBox.Text))
            {
                if (!decimal.TryParse(
                        ServiceCostTextBox.Text.Trim(),
                        out decimal parsedCost))
                {
                    MessageBox.Show(
                        "Please enter a valid Service Cost.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                serviceCost = parsedCost;
            }

            using var db = CreateDbContext();

            var serviceRecord = db.ServiceRecords
                .FirstOrDefault(x =>
                    x.ServiceRecordId == _serviceRecordId);

            if (serviceRecord == null)
            {
                MessageBox.Show(
                    "Service record not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            serviceRecord.AssetId =
                (int)AssetComboBox.SelectedValue;

            serviceRecord.ComplaintDate =
                ComplaintDatePicker.SelectedDate.Value;

            serviceRecord.ProblemDescription =
                ProblemDescriptionTextBox.Text.Trim();

            serviceRecord.ServiceDate =
                ServiceDatePicker.SelectedDate;

            serviceRecord.ServiceProvider =
                string.IsNullOrWhiteSpace(
                    ServiceProviderTextBox.Text)
                    ? null
                    : ServiceProviderTextBox.Text.Trim();

            serviceRecord.ServiceCost = serviceCost;

            serviceRecord.WorkDescription =
                string.IsNullOrWhiteSpace(
                    WorkDescriptionTextBox.Text)
                    ? null
                    : WorkDescriptionTextBox.Text.Trim();

            serviceRecord.Status =
                StatusComboBox.SelectedItem is ComboBoxItem statusItem
                    ? statusItem.Content?.ToString() ?? "Open"
                    : "Open";

            serviceRecord.NextServiceDate =
                NextServiceDatePicker.SelectedDate;

            db.SaveChanges();

            MessageBox.Show(
                "Service record updated successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to update service record.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}