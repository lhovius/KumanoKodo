using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace KumanoKodo.ViewModels;

public partial class ProgressViewModel : ObservableObject
{
    private readonly IDataAccess _dataAccess;
    private readonly ILogger<ProgressViewModel> _logger;

    [ObservableProperty]
    private int completedLessonsCount;

    [ObservableProperty]
    private int totalLessonsCount;

    [ObservableProperty]
    private double progressPercentage;

    public ProgressViewModel(IDataAccess dataAccess, ILogger<ProgressViewModel> logger)
    {
        _dataAccess = dataAccess;
        _logger = logger;
    }

    public async Task LoadProgressAsync(int userId)
    {
        try
        {
            // TODO: Implement progress loading from database
            CompletedLessonsCount = 0;
            TotalLessonsCount = 0;
            ProgressPercentage = 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading progress");
        }
    }
} 