using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using KumanoKodo.Views;

namespace KumanoKodo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDataAccess _dataAccess;

    [ObservableProperty]
    private Page? _currentPage;

    public MainViewModel(IDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
        // Set initial page
        NavigateToHome();
    }

    [RelayCommand]
    private void NavigateToHome()
    {
        CurrentPage = new HomePage();
    }

    [RelayCommand]
    private void NavigateToLessons()
    {
        CurrentPage = new LessonsPage();
    }

    [RelayCommand]
    private void NavigateToProgress()
    {
        CurrentPage = new ProgressPage();
    }

    [RelayCommand]
    private void NavigateToQuiz()
    {
        CurrentPage = new QuizPage();
    }
} 