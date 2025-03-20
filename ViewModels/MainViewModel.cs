using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace KumanoKodo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private Page? _currentPage;

    [RelayCommand]
    private void NavigateToHome()
    {
        CurrentPage = new Views.HomePage();
    }

    [RelayCommand]
    private void NavigateToLessons()
    {
        CurrentPage = new Views.LessonsPage();
    }

    [RelayCommand]
    private void NavigateToProgress()
    {
        CurrentPage = new Views.ProgressPage();
    }

    public MainViewModel()
    {
        // Set initial page
        NavigateToHome();
    }
} 