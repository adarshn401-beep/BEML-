using System;
using System.Linq;
using System.Windows;
using BEMLPropertyManagement.Data;
using BEMLPropertyManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class AddEmployeeWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public AddEmployeeWindow()
    {
        InitializeComponent();

        LoadDepartments();

        StatusComboBox.SelectedIndex = 0;
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

    private void SaveEmployeeButton_Click(
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

            var existingEmployee = db.Employees
                .FirstOrDefault(x =>
                    x.EmployeeCode.ToLower() ==
                    employeeCode.ToLower());

            if (existingEmployee != null)
            {
                MessageBox.Show(
                    "An employee with this code already exists.",
                    "Duplicate Employee Code",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var employee = new Employee
            {
                EmployeeCode = employeeCode,
                EmployeeName = employeeName,
                Designation = designation,
                DepartmentId = departmentId,
                Phone = phone,
                Email = email,
                IsActive = isActive
            };

            db.Employees.Add(employee);
            db.SaveChanges();

            MessageBox.Show(
                "Employee saved successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to save employee.\n\n" +
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