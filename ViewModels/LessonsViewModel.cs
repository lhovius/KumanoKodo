using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using KumanoKodo.Models;
using KumanoKodo;

namespace KumanoKodo.ViewModels
{
    public partial class LessonsViewModel : ObservableObject
    {
        public ObservableCollection<GrammarTopicData> GrammarTopics { get; } = new();
        public ICommand OpenGrammarPdfCommand { get; }
        public ICommand LoadLessonsCommand { get; }
        public ICommand MarkLessonCompletedCommand { get; }

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private LessonData? selectedLesson;

        private readonly DataAccess _dataAccess;

        public LessonsViewModel(DataAccess dataAccess)
        {
            _dataAccess = dataAccess;
            OpenGrammarPdfCommand = new RelayCommand<string>(OpenGrammarPdf);
            LoadLessonsCommand = new RelayCommand(async () => await LoadLessonsAsync());
            MarkLessonCompletedCommand = new RelayCommand<int>(async (lessonId) => await MarkLessonCompletedAsync(lessonId));
        }

        private async Task LoadLessonsAsync()
        {
            try
            {
                IsLoading = true;
                // Implementation will be added later
                await Task.Delay(100); // Placeholder for actual implementation
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

        private async Task MarkLessonCompletedAsync(int lessonId)
        {
            try
            {
                IsLoading = true;
                // Implementation will be added later
                await Task.Delay(100); // Placeholder for actual implementation
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error marking lesson as completed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadGrammarTopicsAsync()
        {
            if (SelectedLesson == null) return;

            try
            {
                IsLoading = true;
                var topics = await _dataAccess.GetGrammarTopicsAsync(SelectedLesson.LessonId);
                GrammarTopics.Clear();
                foreach (var topic in topics)
                {
                    GrammarTopics.Add(topic);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading grammar topics: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSelectedLessonChanged(LessonData? value)
        {
            if (value != null)
            {
                _ = LoadGrammarTopicsAsync();
            }
        }

        private void OpenGrammarPdf(string? pdfLink)
        {
            if (string.IsNullOrEmpty(pdfLink))
            {
                StatusMessage = "Error: PDF link is missing";
                return;
            }

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = pdfLink,
                        UseShellExecute = true
                    }
                };
                process.Start();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening PDF: {ex.Message}";
            }
        }
    }

    public class LessonData
    {
        public int LessonId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        public bool IsCompleted { get; set; }
    }
} 