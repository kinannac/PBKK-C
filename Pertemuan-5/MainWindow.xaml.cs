using System.Windows;
using StudentManager.ViewModels;

namespace StudentManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new StudentViewModel();
    }
}
