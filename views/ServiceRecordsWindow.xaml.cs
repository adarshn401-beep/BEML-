using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class ServiceRecordsWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public ServiceRecordsWindow()
    {
        InitializeComponent();
        LoadServiceRecords();
    }

    private AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        return new AppDbContext(options);
    }

    private void LoadServiceRecords()
    {
        try
        {
            using var db = CreateDbContext();

            var records =
                (
                    from service in db.ServiceRecords
                    join asset in db.Assets
                        on service.AssetId equals asset.AssetId
                    select new
                    {
                        service.ServiceRecordId,
                        asset.ItemNumber,
                        service.ComplaintDate,
                        service.ProblemDescription,
                        service.ServiceDate,
                        service.ServiceProvider,
                        service.ServiceCost,
                        service.Status,
                        service.NextServiceDate
                    }
                ).ToList();

            var displayRecords =
                records.Select(service =>
                {
                    string status =
                        service.Status ?? "Unknown";

                    string nextServiceStatus = "-";

                    if (service.NextServiceDate.HasValue)
                    {
                        DateTime nextDate =
                            service.NextServiceDate.Value.Date;

                        DateTime today =
                            DateTime.Today;

                        if (nextDate < today)
                        {
                            nextServiceStatus =
                                "OVERDUE";
                        }
                        else if (nextDate <= today.AddDays(30))
                        {
                            nextServiceStatus =
                                "DUE SOON";
                        }
                        else
                        {
                            nextServiceStatus =
                                nextDate.ToString("dd-MM-yyyy");
                        }
                    }

                    return new ServiceRecordDisplay
                    {
                        ServiceRecordId =
                            service.ServiceRecordId,

                        ItemNumber =
                            service.ItemNumber,

                        ComplaintDate =
                            service.ComplaintDate
                                .ToString("dd-MM-yyyy"),

                        ProblemDescription =
                            service.ProblemDescription,

                        ServiceDate =
                            service.ServiceDate.HasValue
                                ? service.ServiceDate.Value
                                    .ToString("dd-MM-yyyy")
                                : "-",

                        ServiceProvider =
                            service.ServiceProvider ?? "-",

                        ServiceCost =
                            service.ServiceCost.HasValue
                                ? "₹ " +
                                  service.ServiceCost.Value
                                      .ToString("N2")
                                : "-",

                        Status =
                            status,

                        NextServiceDate =
                            nextServiceStatus
                    };
                }).ToList();

            ServiceDataGrid.ItemsSource =
                displayRecords;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load service records.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddServiceButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var addServiceWindow =
            new AddServiceRecordWindow();

        addServiceWindow.ShowDialog();

        LoadServiceRecords();
    }

    private void ViewServiceButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext
            is not ServiceRecordDisplay selectedRecord)
            return;

        var viewServiceWindow =
            new ServiceRecordDetailsWindow(
                selectedRecord.ServiceRecordId);

        viewServiceWindow.ShowDialog();
    }

    private void EditServiceButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext
            is not ServiceRecordDisplay selectedRecord)
            return;

        var editServiceWindow =
            new EditServiceRecordWindow(
                selectedRecord.ServiceRecordId);

        editServiceWindow.ShowDialog();

        LoadServiceRecords();
    }

    private void DeleteServiceButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext
            is not ServiceRecordDisplay selectedRecord)
            return;

        int serviceRecordId =
            selectedRecord.ServiceRecordId;

        string itemNumber =
            selectedRecord.ItemNumber;

        MessageBoxResult result =
            MessageBox.Show(
                $"Are you sure you want to delete the service record for:\n\n{itemNumber}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            using var db =
                CreateDbContext();

            var serviceRecord =
                db.ServiceRecords
                    .FirstOrDefault(x =>
                        x.ServiceRecordId ==
                        serviceRecordId);

            if (serviceRecord == null)
            {
                MessageBox.Show(
                    "Service record not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                LoadServiceRecords();
                return;
            }

            db.ServiceRecords.Remove(
                serviceRecord);

            db.SaveChanges();

            MessageBox.Show(
                "Service record deleted successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadServiceRecords();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to delete service record.\n\n" +
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
        SearchServiceRecords();
    }

    private void SearchTextBox_KeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchServiceRecords();
        }
    }

    private void SearchServiceRecords()
    {
        try
        {
            string searchText =
                SearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadServiceRecords();
                return;
            }

            using var db =
                CreateDbContext();

            var records =
                (
                    from service in db.ServiceRecords

                    join asset in db.Assets
                        on service.AssetId equals asset.AssetId

                    where
                        asset.ItemNumber.Contains(searchText)
                        ||
                        service.ProblemDescription
                            .Contains(searchText)
                        ||
                        (
                            service.ServiceProvider != null &&
                            service.ServiceProvider
                                .Contains(searchText)
                        )
                        ||
                        service.Status.Contains(searchText)

                    select new
                    {
                        service.ServiceRecordId,
                        asset.ItemNumber,
                        service.ComplaintDate,
                        service.ProblemDescription,
                        service.ServiceDate,
                        service.ServiceProvider,
                        service.ServiceCost,
                        service.Status,
                        service.NextServiceDate
                    }
                ).ToList();

            if (records.Count == 0)
            {
                ServiceDataGrid.ItemsSource = null;

                MessageBox.Show(
                    "No service records found.",
                    "Search",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var displayRecords =
                records.Select(service =>
                {
                    string nextServiceStatus = "-";

                    if (service.NextServiceDate.HasValue)
                    {
                        DateTime nextDate =
                            service.NextServiceDate.Value.Date;

                        if (nextDate < DateTime.Today)
                        {
                            nextServiceStatus =
                                "OVERDUE";
                        }
                        else if (nextDate <=
                                 DateTime.Today.AddDays(30))
                        {
                            nextServiceStatus =
                                "DUE SOON";
                        }
                        else
                        {
                            nextServiceStatus =
                                nextDate.ToString(
                                    "dd-MM-yyyy");
                        }
                    }

                    return new ServiceRecordDisplay
                    {
                        ServiceRecordId =
                            service.ServiceRecordId,

                        ItemNumber =
                            service.ItemNumber,

                        ComplaintDate =
                            service.ComplaintDate
                                .ToString("dd-MM-yyyy"),

                        ProblemDescription =
                            service.ProblemDescription,

                        ServiceDate =
                            service.ServiceDate.HasValue
                                ? service.ServiceDate.Value
                                    .ToString("dd-MM-yyyy")
                                : "-",

                        ServiceProvider =
                            service.ServiceProvider ?? "-",

                        ServiceCost =
                            service.ServiceCost.HasValue
                                ? "₹ " +
                                  service.ServiceCost.Value
                                      .ToString("N2")
                                : "-",

                        Status =
                            service.Status,

                        NextServiceDate =
                            nextServiceStatus
                    };
                }).ToList();

            ServiceDataGrid.ItemsSource =
                displayRecords;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to search service records.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private class ServiceRecordDisplay
    {
        public int ServiceRecordId { get; set; }

        public string ItemNumber { get; set; } =
            string.Empty;

        public string ComplaintDate { get; set; } =
            string.Empty;

        public string ProblemDescription { get; set; } =
            string.Empty;

        public string ServiceDate { get; set; } =
            string.Empty;

        public string ServiceProvider { get; set; } =
            string.Empty;

        public string ServiceCost { get; set; } =
            string.Empty;

        public string Status { get; set; } =
            string.Empty;

        public string NextServiceDate { get; set; } =
            string.Empty;
    }
}