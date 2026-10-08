using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class EmployeesWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public EmployeesWindow()
    {
        InitializeComponent();
        LoadEmployees();
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private void LoadEmployees()
    {
        try
        {
            using var db = CreateDbContext();

            var employees = (
                from employee in db.Employees
                join department in db.Departments
                    on employee.DepartmentId equals department.DepartmentId
                    into departmentGroup
                from department in departmentGroup.DefaultIfEmpty()
                select new EmployeeDisplay
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.EmployeeName,
                    Designation = employee.Designation ?? "-",
                    DepartmentName = department != null
                        ? department.DepartmentName
                        : "-",
                    Phone = employee.Phone ?? "-",
                    Email = employee.Email ?? "-",
                    Status = employee.IsActive
                        ? "Active"
                        : "Inactive"
                }
            )
            .OrderBy(x => x.EmployeeName)
            .ToList();

            EmployeeDataGrid.ItemsSource = employees;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load employees.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddEmployeeButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var addEmployeeWindow =
            new AddEmployeeWindow();

        addEmployeeWindow.ShowDialog();

        LoadEmployees();
    }

    private void ViewEmployeeButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not EmployeeDisplay selectedEmployee)
            return;

        MessageBox.Show(
            $"Employee Details\n\n" +
            $"Employee Code: {selectedEmployee.EmployeeCode}\n" +
            $"Employee Name: {selectedEmployee.EmployeeName}\n" +
            $"Designation: {selectedEmployee.Designation}\n" +
            $"Department: {selectedEmployee.DepartmentName}\n" +
            $"Phone: {selectedEmployee.Phone}\n" +
            $"Email: {selectedEmployee.Email}\n" +
            $"Status: {selectedEmployee.Status}",
            "Employee Details",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void EditEmployeeButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not EmployeeDisplay selectedEmployee)
            return;

        var editEmployeeWindow =
            new EditEmployeeWindow(
                selectedEmployee.EmployeeId);

        editEmployeeWindow.ShowDialog();

        LoadEmployees();
    }

    private void DeleteEmployeeButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not EmployeeDisplay selectedEmployee)
            return;

        MessageBoxResult result = MessageBox.Show(
            $"Are you sure you want to delete this employee?\n\n" +
            $"Employee Code: {selectedEmployee.EmployeeCode}\n" +
            $"Employee Name: {selectedEmployee.EmployeeName}",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = CreateDbContext();

            var employee = db.Employees
                .FirstOrDefault(x =>
                    x.EmployeeId == selectedEmployee.EmployeeId);

            if (employee == null)
            {
                MessageBox.Show(
                    "Employee not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                LoadEmployees();
                return;
            }

            db.Employees.Remove(employee);
            db.SaveChanges();

            MessageBox.Show(
                "Employee deleted successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadEmployees();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to delete employee.\n\n" +
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
        SearchEmployees();
    }

    private void SearchTextBox_KeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchEmployees();
        }
    }

    private void SearchEmployees()
    {
        try
        {
            string searchText =
                SearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText) ||
                searchText == "Search employee...")
            {
                LoadEmployees();
                return;
            }

            using var db = CreateDbContext();

            var employees = (
                from employee in db.Employees
                join department in db.Departments
                    on employee.DepartmentId equals department.DepartmentId
                    into departmentGroup
                from department in departmentGroup.DefaultIfEmpty()
                where employee.EmployeeCode.Contains(searchText)
                    || employee.EmployeeName.Contains(searchText)
                    || (employee.Designation != null &&
                        employee.Designation.Contains(searchText))
                    || (department != null &&
                        department.DepartmentName.Contains(searchText))
                    || (employee.Phone != null &&
                        employee.Phone.Contains(searchText))
                    || (employee.Email != null &&
                        employee.Email.Contains(searchText))
                select new EmployeeDisplay
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.EmployeeName,
                    Designation = employee.Designation ?? "-",
                    DepartmentName = department != null
                        ? department.DepartmentName
                        : "-",
                    Phone = employee.Phone ?? "-",
                    Email = employee.Email ?? "-",
                    Status = employee.IsActive
                        ? "Active"
                        : "Inactive"
                }
            )
            .OrderBy(x => x.EmployeeName)
            .ToList();

            EmployeeDataGrid.ItemsSource = employees;

            if (employees.Count == 0)
            {
                MessageBox.Show(
                    "No employees found.",
                    "Search",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to search employees.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private class EmployeeDisplay
    {
        public int EmployeeId { get; set; }

        public string EmployeeCode { get; set; } =
            string.Empty;

        public string EmployeeName { get; set; } =
            string.Empty;

        public string Designation { get; set; } =
            string.Empty;

        public string DepartmentName { get; set; } =
            string.Empty;

        public string Phone { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public string Status { get; set; } =
            string.Empty;
    }
}