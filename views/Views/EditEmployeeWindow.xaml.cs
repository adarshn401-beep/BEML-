using System;
using System.Linq;
using System.Windows;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class EditEmployeeWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly int employeeId;

    public EditEmployeeWindow(int employeeId)
    {
        InitializeComponent();

        this.employeeId = employeeId;

        LoadDepartments();
        StatusComboBox.SelectedIndex = 0;
        LoadEmployee();
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private void LoadDepartments()
    {
        try
        {
            using var db = CreateDbContext();

            var departments = db.Departments
                .OrderBy(x => x.DepartmentName)
                .ToList();

            DepartmentComboBox.ItemsSource = departments;
            DepartmentComboBox.DisplayMemberPath =
                "DepartmentName";
            DepartmentComboBox.SelectedValuePath =
                "DepartmentId";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load departments.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void LoadEmployee()
    {
        try
        {
            using var db = CreateDbContext();

            var employee = db.Employees
                .FirstOrDefault(x =>
                    x.EmployeeId == employeeId);

            if (employee == null)
            {
                MessageBox.Show(
                    "Employee not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
                return;
            }

            EmployeeCodeTextBox.Text =
                employee.EmployeeCode;

            EmployeeNameTextBox.Text =
                employee.EmployeeName;

            DesignationTextBox.Text =
                employee.Designation ?? string.Empty;

            DepartmentComboBox.SelectedValue =
                employee.DepartmentId;

            PhoneTextBox.Text =
                employee.Phone ?? string.Empty;

            EmailTextBox.Text =
                employee.Email ?? string.Empty;

            StatusComboBox.SelectedIndex =
                employee.IsActive ? 0 : 1;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load employee.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SaveChangesButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string employeeCode =
            EmployeeCodeTextBox.Text.Trim();

        string employeeName =
            EmployeeNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            MessageBox.Show(
                "Please enter the employee code.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (string.IsNullOrWhiteSpace(employeeName))
        {
            MessageBox.Show(
                "Please enter the employee name.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        int? departmentId = null;

        if (DepartmentComboBox.SelectedValue != null)
        {
            departmentId =
                (int)DepartmentComboBox.SelectedValue;
        }

        string? designation =
            string.IsNullOrWhiteSpace(
                DesignationTextBox.Text)
                ? null
                : DesignationTextBox.Text.Trim();

        string? phone =
            string.IsNullOrWhiteSpace(
                PhoneTextBox.Text)
                ? null
                : PhoneTextBox.Text.Trim();

        string? email =
            string.IsNullOrWhiteSpace(
                EmailTextBox.Text)
                ? null
                : EmailTextBox.Text.Trim();

        bool isActive = true;

        if (StatusComboBox.SelectedItem
            is System.Windows.Controls.ComboBoxItem selectedItem)
        {
            string status =
                selectedItem.Content?.ToString()
                ?? "Active";

            isActive = status == "Active";
        }

        try
        {
            using var db = CreateDbContext();

            var employee = db.Employees
                .FirstOrDefault(x =>
                    x.EmployeeId == employeeId);

            if (employee == null)
            {
                MessageBox.Show(
                    "Employee not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            bool duplicateExists = db.Employees
                .Any(x =>
                    x.EmployeeId != employeeId &&
                    x.EmployeeCode.ToLower() ==
                    employeeCode.ToLower());

            if (duplicateExists)
            {
                MessageBox.Show(
                    "Another employee with this code already exists.",
                    "Duplicate Employee Code",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            employee.EmployeeCode =
                employeeCode;

            employee.EmployeeName =
                employeeName;

            employee.Designation =
                designation;

            employee.DepartmentId =
                departmentId;

            employee.Phone =
                phone;

            employee.Email =
                email;

            employee.IsActive =
                isActive;

            db.SaveChanges();

            MessageBox.Show(
                "Employee updated successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to update employee.\n\n" +
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