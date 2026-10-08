using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class EditComplaintWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly int complaintId;

    public EditComplaintWindow(int complaintId)
    {
        InitializeComponent();

        this.complaintId = complaintId;

        LoadAssets();
        LoadEmployees();
        LoadComplaint();
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

    private void LoadEmployees()
    {
        try
        {
            using var db = CreateDbContext();

            EmployeeComboBox.ItemsSource = db.Employees
                .Where(x => x.IsActive)
                .OrderBy(x => x.EmployeeName)
                .ToList();

            EmployeeComboBox.DisplayMemberPath = "EmployeeName";
            EmployeeComboBox.SelectedValuePath = "EmployeeId";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load employees.\n\n" + ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void LoadComplaint()
    {
        try
        {
            using var db = CreateDbContext();

            var complaint = db.Complaints
                .FirstOrDefault(x =>
                    x.ComplaintId == complaintId);

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

            AssetComboBox.SelectedValue = complaint.AssetId;

            if (complaint.EmployeeId.HasValue)
            {
                EmployeeComboBox.SelectedValue =
                    complaint.EmployeeId.Value;
            }

            ComplaintDatePicker.SelectedDate =
                complaint.ComplaintDate;

            ProblemTitleTextBox.Text =
                complaint.ProblemTitle;

            ProblemDescriptionTextBox.Text =
                complaint.ProblemDescription;

            SelectComboBoxItem(
                PriorityComboBox,
                complaint.Priority);

            SelectComboBoxItem(
                StatusComboBox,
                complaint.Status);

            ResolvedDatePicker.SelectedDate =
                complaint.ResolvedDate;

            ResolutionNotesTextBox.Text =
                complaint.ResolutionNotes ?? string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load complaint.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SelectComboBoxItem(
        ComboBox comboBox,
        string value)
    {
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

    private void SaveChangesButton_Click(
        object sender,
        RoutedEventArgs e)
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
                    ProblemTitleTextBox.Text))
            {
                MessageBox.Show(
                    "Please enter Problem Title.",
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

            string priority = "Medium";

            if (PriorityComboBox.SelectedItem
                is ComboBoxItem priorityItem)
            {
                priority =
                    priorityItem.Content?.ToString()
                    ?? "Medium";
            }

            string status = "Open";

            if (StatusComboBox.SelectedItem
                is ComboBoxItem statusItem)
            {
                status =
                    statusItem.Content?.ToString()
                    ?? "Open";
            }

            using var db = CreateDbContext();

            var complaint = db.Complaints
                .FirstOrDefault(x =>
                    x.ComplaintId == complaintId);

            if (complaint == null)
            {
                MessageBox.Show(
                    "Complaint not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            complaint.AssetId =
                (int)AssetComboBox.SelectedValue;

            complaint.EmployeeId =
                EmployeeComboBox.SelectedValue != null
                    ? (int?)EmployeeComboBox.SelectedValue
                    : null;

            complaint.ComplaintDate =
                ComplaintDatePicker.SelectedDate.Value;

            complaint.ProblemTitle =
                ProblemTitleTextBox.Text.Trim();

            complaint.ProblemDescription =
                ProblemDescriptionTextBox.Text.Trim();

            complaint.Priority = priority;

            complaint.Status = status;

            complaint.ResolvedDate =
                ResolvedDatePicker.SelectedDate;

            complaint.ResolutionNotes =
                string.IsNullOrWhiteSpace(
                    ResolutionNotesTextBox.Text)
                    ? null
                    : ResolutionNotesTextBox.Text.Trim();

            db.SaveChanges();

            MessageBox.Show(
                "Complaint updated successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to update complaint.\n\n" +
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