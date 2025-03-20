using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KumanoKodo.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace KumanoKodo.ViewModels
{
    public partial class LessonsViewModel : ObservableObject
    {
        private readonly DataAccess _dataAccess;
        private readonly AzureBlobService _blobService;
        private readonly int _userId;

        [ObservableProperty]
        private ObservableCollection<LessonItem> _lessons;

        [ObservableProperty]
        private LessonItem _selectedLesson;

        [ObservableProperty]
        private ObservableCollection<VocabularyItem> _vocabularyList;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage;

        public LessonsViewModel(DataAccess dataAccess, AzureBlobService blobService, int userId)
        {
            _dataAccess = dataAccess;
            _blobService = blobService;
            _userId = userId;
            _lessons = new ObservableCollection<LessonItem>();
            _vocabularyList = new ObservableCollection<VocabularyItem>();
            LoadLessonsAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        private async Task LoadLessonsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading lessons...";

                var lessons = await _dataAccess.GetLessonsAsync(_userId);
                Lessons.Clear();
                foreach (var lesson in lessons)
                {
                    Lessons.Add(new LessonItem
                    {
                        Id = lesson.Id,
                        Title = lesson.Title,
                        Description = lesson.Description,
                        ImageUrl = lesson.ImageUrl,
                        AudioUrl = lesson.AudioUrl,
                        IsCompleted = lesson.IsCompleted
                    });
                }

                StatusMessage = $"Loaded {Lessons.Count} lessons";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading lessons: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadVocabularyAsync(int lessonId)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading vocabulary...";

                var vocabulary = await _dataAccess.GetVocabularyAsync(lessonId);
                VocabularyList.Clear();
                foreach (var word in vocabulary)
                {
                    VocabularyList.Add(new VocabularyItem
                    {
                        Id = word.Id,
                        Word = word.Word,
                        Meaning = word.Meaning,
                        ImageUrl = word.ImageUrl,
                        AudioUrl = word.AudioUrl
                    });
                }

                StatusMessage = $"Loaded {VocabularyList.Count} vocabulary items";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading vocabulary: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task MarkLessonCompletedAsync(int lessonId)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Updating lesson progress...";

                await _dataAccess.MarkLessonCompletedAsync(_userId, lessonId);
                await LoadLessonsAsync();

                StatusMessage = "Lesson marked as completed";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating lesson progress: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class LessonItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class VocabularyItem
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public string Meaning { get; set; }
        public string ImageUrl { get; set; }
        public string AudioUrl { get; set; }
    }
} 