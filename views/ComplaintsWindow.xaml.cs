using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class ComplaintsWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public ComplaintsWindow()
    {
        InitializeComponent();
        LoadComplaints();
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private void LoadComplaints()
    {
        try
        {
            using var db = CreateDbContext();

            var complaints = (
                from complaint in db.Complaints
                join asset in db.Assets
                    on complaint.AssetId equals asset.AssetId
                join employee in db.Employees
                    on complaint.EmployeeId equals employee.EmployeeId
                    into employeeGroup
                from employee in employeeGroup.DefaultIfEmpty()
                select new
                {
                    complaint.ComplaintId,
                    asset.ItemNumber,
                    complaint.ComplaintDate,
                    complaint.ProblemTitle,
                    complaint.Priority,
                    complaint.Status,
                    EmployeeName = employee != null
                        ? employee.EmployeeName
                        : "-",
                    complaint.ResolvedDate
                }
            ).ToList();

            var displayComplaints = complaints.Select(complaint =>
                new ComplaintDisplay
                {
                    ComplaintId = complaint.ComplaintId,
                    ItemNumber = complaint.ItemNumber,
                    ComplaintDate = complaint.ComplaintDate
                        .ToString("dd-MM-yyyy"),
                    ProblemTitle = complaint.ProblemTitle,
                    Priority = complaint.Priority,
                    Status = complaint.Status,
                    EmployeeName = complaint.EmployeeName,
                    ResolvedDate = complaint.ResolvedDate.HasValue
                        ? complaint.ResolvedDate.Value
                            .ToString("dd-MM-yyyy")
                        : "-"
                }).ToList();

            ComplaintDataGrid.ItemsSource = displayComplaints;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load complaints.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddComplaintButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var addComplaintWindow = new AddComplaintWindow();

        addComplaintWindow.ShowDialog();

        LoadComplaints();
    }

    private void ViewComplaintButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not ComplaintDisplay selectedComplaint)
            return;

        var complaintDetailsWindow =
            new ComplaintDetailsWindow(
                selectedComplaint.ComplaintId);

        complaintDetailsWindow.ShowDialog();
    }

    private void EditComplaintButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not ComplaintDisplay selectedComplaint)
            return;

        var editComplaintWindow =
            new EditComplaintWindow(
                selectedComplaint.ComplaintId);

        editComplaintWindow.ShowDialog();

        LoadComplaints();
    }

    private void DeleteComplaintButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not ComplaintDisplay selectedComplaint)
            return;

        MessageBoxResult result = MessageBox.Show(
            $"Are you sure you want to delete the complaint for:\n\n" +
            $"{selectedComplaint.ItemNumber}?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = CreateDbContext();

            var complaint = db.Complaints
                .FirstOrDefault(x =>
                    x.ComplaintId == selectedComplaint.ComplaintId);

            if (complaint == null)
            {
                MessageBox.Show(
                    "Complaint not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                LoadComplaints();
                return;
            }

            db.Complaints.Remove(complaint);
            db.SaveChanges();

            MessageBox.Show(
                "Complaint deleted successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadComplaints();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to delete complaint.\n\n" +
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
        SearchComplaints();
    }

    private void SearchTextBox_KeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchComplaints();
        }
    }

    private void SearchComplaints()
    {
        try
        {
            string searchText = SearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText) ||
                searchText == "Search complaints...")
            {
                LoadComplaints();
                return;
            }

            using var db = CreateDbContext();

            var complaints = (
                from complaint in db.Complaints
                join asset in db.Assets
                    on complaint.AssetId equals asset.AssetId
                join employee in db.Employees
                    on complaint.EmployeeId equals employee.EmployeeId
                    into employeeGroup
                from employee in employeeGroup.DefaultIfEmpty()
                where asset.ItemNumber.Contains(searchText)
                    || complaint.ProblemTitle.Contains(searchText)
                    || complaint.ProblemDescription.Contains(searchText)
                    || complaint.Priority.Contains(searchText)
                    || complaint.Status.Contains(searchText)
                    || (employee != null &&
                        employee.EmployeeName.Contains(searchText))
                select new
                {
                    complaint.ComplaintId,
                    asset.ItemNumber,
                    complaint.ComplaintDate,
                    complaint.ProblemTitle,
                    complaint.Priority,
                    complaint.Status,
                    EmployeeName = employee != null
                        ? employee.EmployeeName
                        : "-",
                    complaint.ResolvedDate
                }
            ).ToList();

            var displayComplaints = complaints.Select(complaint =>
                new ComplaintDisplay
                {
                    ComplaintId = complaint.ComplaintId,
                    ItemNumber = complaint.ItemNumber,
                    ComplaintDate = complaint.ComplaintDate
                        .ToString("dd-MM-yyyy"),
                    ProblemTitle = complaint.ProblemTitle,
                    Priority = complaint.Priority,
                    Status = complaint.Status,
                    EmployeeName = complaint.EmployeeName,
                    ResolvedDate = complaint.ResolvedDate.HasValue
                        ? complaint.ResolvedDate.Value
                            .ToString("dd-MM-yyyy")
                        : "-"
                }).ToList();

            ComplaintDataGrid.ItemsSource = displayComplaints;

            if (displayComplaints.Count == 0)
            {
                MessageBox.Show(
                    "No complaints found.",
                    "Search",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to search complaints.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private class ComplaintDisplay
    {
        public int ComplaintId { get; set; }

        public string ItemNumber { get; set; } = string.Empty;

        public string ComplaintDate { get; set; } = string.Empty;

        public string ProblemTitle { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public string ResolvedDate { get; set; } = string.Empty;
    }
}