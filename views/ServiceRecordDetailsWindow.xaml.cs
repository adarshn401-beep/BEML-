using System;
using System.Linq;
using System.Windows;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class ServiceRecordDetailsWindow : Window
{
    private readonly int _serviceRecordId;

    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public ServiceRecordDetailsWindow(int serviceRecordId)
    {
        InitializeComponent();

        _serviceRecordId = serviceRecordId;

        LoadServiceRecordDetails();
    }

    private void LoadServiceRecordDetails()
    {
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            using var db = new AppDbContext(options);

            var record = (
                from service in db.ServiceRecords
                join asset in db.Assets
                    on service.AssetId equals asset.AssetId
                where service.ServiceRecordId == _serviceRecordId
                select new
                {
                    Asset = asset.ItemNumber,
                    service.ComplaintDate,
                    service.ProblemDescription,
                    service.ServiceDate,
                    service.ServiceProvider,
                    service.ServiceCost,
                    service.WorkDescription,
                    service.Status,
                    service.NextServiceDate,
                    service.Remarks
                }
            ).FirstOrDefault();

            if (record == null)
            {
                MessageBox.Show(
                    "Service record not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
                return;
            }

            AssetTextBlock.Text = record.Asset;

            ComplaintDateTextBlock.Text =
                record.ComplaintDate.ToString("dd-MM-yyyy");

            ProblemDescriptionTextBlock.Text =
                string.IsNullOrWhiteSpace(record.ProblemDescription)
                    ? "-"
                    : record.ProblemDescription;

            ServiceDateTextBlock.Text =
                record.ServiceDate.HasValue
                    ? record.ServiceDate.Value.ToString("dd-MM-yyyy")
                    : "-";

            ServiceProviderTextBlock.Text =
                string.IsNullOrWhiteSpace(record.ServiceProvider)
                    ? "-"
                    : record.ServiceProvider;

            ServiceCostTextBlock.Text =
                record.ServiceCost.HasValue
                    ? "₹ " + record.ServiceCost.Value.ToString("N2")
                    : "-";

            WorkDescriptionTextBlock.Text =
                string.IsNullOrWhiteSpace(record.WorkDescription)
                    ? "-"
                    : record.WorkDescription;

            StatusTextBlock.Text = record.Status;

            NextServiceDateTextBlock.Text =
                record.NextServiceDate.HasValue
                    ? record.NextServiceDate.Value.ToString("dd-MM-yyyy")
                    : "-";

            RemarksTextBlock.Text =
                string.IsNullOrWhiteSpace(record.Remarks)
                    ? "-"
                    : record.Remarks;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load service record details.\n\n" + ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}