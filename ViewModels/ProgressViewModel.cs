using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KumanoKodo.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace KumanoKodo.ViewModels
{
    public partial class ProgressViewModel : ObservableObject
    {
        private readonly DataAccess _dataAccess;
        private readonly AzureBlobService _blobService;
        private readonly int _userId;

        [ObservableProperty]
        private ObservableCollection<LessonProgress> _lessonProgress;

        [ObservableProperty]
        private ObservableCollection<QuizProgress> _quizProgress;

        [ObservableProperty]
        private string _progressMapUrl;

        [ObservableProperty]
        private int _completedLessons;

        [ObservableProperty]
        private int _totalLessons;

        [ObservableProperty]
        private int _correctQuizzes;

        [ObservableProperty]
        private int _totalQuizzes;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage;

        public ProgressViewModel(DataAccess dataAccess, AzureBlobService blobService, int userId)
        {
            _dataAccess = dataAccess;
            _blobService = blobService;
            _userId = userId;
            _lessonProgress = new ObservableCollection<LessonProgress>();
            _quizProgress = new ObservableCollection<QuizProgress>();
            LoadProgressAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        private async Task LoadProgressAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading progress data...";

                // Load lesson progress
                var lessons = await _dataAccess.GetLessonProgressAsync(_userId);
                LessonProgress.Clear();
                foreach (var lesson in lessons)
                {
                    LessonProgress.Add(new LessonProgress
                    {
                        LessonId = lesson.LessonId,
                        Title = lesson.Title,
                        CompletedDate = lesson.CompletedDate,
                        IsCompleted = lesson.IsCompleted
                    });
                }

                // Load quiz progress
                var quizzes = await _dataAccess.GetQuizProgressAsync(_userId);
                QuizProgress.Clear();
                foreach (var quiz in quizzes)
                {
                    QuizProgress.Add(new QuizProgress
                    {
                        QuizId = quiz.QuizId,
                        LastAttempted = quiz.LastAttempted,
                        CorrectAttempts = quiz.CorrectAttempts,
                        IncorrectAttempts = quiz.IncorrectAttempts,
                        NextReviewDate = quiz.NextReviewDate
                    });
                }

                // Update summary statistics
                CompletedLessons = LessonProgress.Count(l => l.IsCompleted);
                TotalLessons = LessonProgress.Count;
                CorrectQuizzes = QuizProgress.Sum(q => q.CorrectAttempts);
                TotalQuizzes = QuizProgress.Sum(q => q.CorrectAttempts + q.IncorrectAttempts);

                // Load progress map
                ProgressMapUrl = _blobService.GetProgressImageUrl(_userId);

                StatusMessage = "Progress data loaded successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading progress: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshProgressAsync()
        {
            await LoadProgressAsync();
        }

        partial class LessonProgress
        {
            public int LessonId { get; set; }
            public string Title { get; set; }
            public DateTime? CompletedDate { get; set; }
            public bool IsCompleted { get; set; }
        }

        partial class QuizProgress
        {
            public int QuizId { get; set; }
            public DateTime LastAttempted { get; set; }
            public int CorrectAttempts { get; set; }
            public int IncorrectAttempts { get; set; }
            public DateTime NextReviewDate { get; set; }
        }
    }
} 