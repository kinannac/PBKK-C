using System.Windows;
using StudentRegistration.Repositories;
namespace StudentRegistration;

public partial class MainWindow : Window
{
    private readonly StudentRepository _studentRepository;
    public MainWindow()
    {
        InitializeComponent();
        _studentRepository = new StudentRepository();
        LoadStudents();
    }
    private void LoadStudents()
    {
        try
        {
            var students = _studentRepository.GetAll();
            StudentDataGrid.ItemsSource = students;
        
}
        catch (Exception ex)
        {
            MessageBox.Show(
            ex.Message,
            "Database Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        }
    }
    private void RefreshButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        LoadStudents();
    }
}