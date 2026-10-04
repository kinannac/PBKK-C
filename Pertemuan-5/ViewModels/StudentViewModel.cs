using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StudentManager.Data;
using StudentManager.Models;

namespace StudentManager.ViewModels;

public class StudentViewModel : INotifyPropertyChanged
{
    private readonly StudentRepository _repository;

    public ObservableCollection<Student> Students { get; } = new();

    private Student? _selectedStudent;
    public Student? SelectedStudent
    {
        get => _selectedStudent;
        set
        {
            _selectedStudent = value;
            OnPropertyChanged();
        }
    }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
        }
    }

    public int TotalStudents => Students.Count;
    public int TotalInformatika =>
        Students.Count(x => x.Jurusan == "Informatika");
    public int TotalSistemInformasi =>
        Students.Count(x => x.Jurusan == "Sistem Informasi");
    public int TotalLakiLaki =>
        Students.Count(x => x.Gender == "Laki-laki");
    public int TotalPerempuan =>
        Students.Count(x => x.Gender == "Perempuan");

    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SearchCommand { get; }

    public StudentViewModel()
    {
        _repository = new StudentRepository();

        SaveCommand = new RelayCommand(Save);
        DeleteCommand = new RelayCommand(Delete);
        ResetCommand = new RelayCommand(Reset);
        SearchCommand = new RelayCommand(Search);

        LoadData();
        Reset(); 
    }

    private void LoadData()
    {
        Students.Clear();

        foreach (var student in _repository.GetAll())
            Students.Add(student);

        RefreshStatistics();
    }

    private void Save()
    {
        if (SelectedStudent == null)
        {
            System.Windows.MessageBox.Show("SelectedStudent masih null!");
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedStudent.NIM) ||
            string.IsNullOrWhiteSpace(SelectedStudent.Nama))
        {
            System.Windows.MessageBox.Show("NIM atau Nama tidak boleh kosong!");
            return;
        }

        try
        {
            if (SelectedStudent.Id == 0)
                _repository.Insert(SelectedStudent);
            else
                _repository.Update(SelectedStudent);

            LoadData();
            Reset();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show("Gagal menyimpan: " + ex.Message);
        }
    }

    private void Delete()
    {
        if (SelectedStudent == null || SelectedStudent.Id == 0) return;

        _repository.Delete(SelectedStudent.Id);

        LoadData();
        Reset();
    }

    private void Search()
    {
        var result = string.IsNullOrWhiteSpace(SearchText)
            ? _repository.GetAll()
            : _repository.Search(SearchText);

        Students.Clear();

        foreach (var student in result)
            Students.Add(student);

        RefreshStatistics();
    }

    private void Reset()
    {
        SelectedStudent = new Student();
    }

    private void RefreshStatistics()
    {
        OnPropertyChanged(nameof(TotalStudents));
        OnPropertyChanged(nameof(TotalInformatika));
        OnPropertyChanged(nameof(TotalSistemInformasi));
        OnPropertyChanged(nameof(TotalLakiLaki));
        OnPropertyChanged(nameof(TotalPerempuan));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
