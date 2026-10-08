using System;
using System.Linq;
using System.Windows;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class ComplaintDetailsWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly int complaintId;

    public ComplaintDetailsWindow(int complaintId)
    {
        InitializeComponent();

        this.complaintId = complaintId;

        LoadComplaint();
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private void LoadComplaint()
    {
        try
        {
            using var db = CreateDbContext();

            var complaint = (
                from c in db.Complaints
                join asset in db.Assets
                    on c.AssetId equals asset.AssetId
                join employee in db.Employees
                    on c.EmployeeId equals employee.EmployeeId
                    into employeeGroup
                from employee in employeeGroup.DefaultIfEmpty()
                where c.ComplaintId == complaintId
                select new
                {
                    c.ComplaintId,
                    asset.ItemNumber,
                    EmployeeName = employee != null
                        ? employee.EmployeeName
                        : "-",
                    c.ComplaintDate,
                    c.ProblemTitle,
                    c.Priority,
                    c.Status,
                    c.ProblemDescription,
                    c.ResolvedDate,
                    c.ResolutionNotes,
                    c.CreatedDate
                }
            ).FirstOrDefault();

            if (complaint == null)
            {
                MessageBox.Show(
                    "Complaint not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
                return;
            }

            AssetTextBlock.Text = complaint.ItemNumber;

            EmployeeTextBlock.Text = complaint.EmployeeName;

            ComplaintDateTextBlock.Text =
                complaint.ComplaintDate.ToString("dd-MM-yyyy");

            ProblemTitleTextBlock.Text =
                complaint.ProblemTitle;

            PriorityTextBlock.Text =
                complaint.Priority;

            StatusTextBlock.Text =
                complaint.Status;

            ProblemDescriptionTextBlock.Text =
                complaint.ProblemDescription;

            ResolvedDateTextBlock.Text =
                complaint.ResolvedDate.HasValue
                    ? complaint.ResolvedDate.Value.ToString("dd-MM-yyyy")
                    : "-";

            ResolutionNotesTextBlock.Text =
                string.IsNullOrWhiteSpace(complaint.ResolutionNotes)
                    ? "-"
                    : complaint.ResolutionNotes;

            CreatedDateTextBlock.Text =
                complaint.CreatedDate.ToString("dd-MM-yyyy HH:mm");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load complaint details.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}