using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace KumanoKodo.ViewModels;

public partial class QuizViewModel : ObservableObject
{
    private readonly DataAccess _dataAccess;
    private List<QuizQuestion> _questions = new();
    private int _currentQuestionIndex = -1;
    private readonly int _userId;
    private readonly int _lessonId;

    [ObservableProperty]
    private string _currentQuestion = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _answers = new();

    [ObservableProperty]
    private bool _isAnswerSelected;

    [ObservableProperty]
    private int _selectedAnswerIndex = -1;

    [ObservableProperty]
    private bool _isCorrect;

    [ObservableProperty]
    private string _feedbackMessage = string.Empty;

    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    private int _totalQuestions;

    [ObservableProperty]
    private bool _isQuizComplete;

    [ObservableProperty]
    private string _progressMessage = string.Empty;

    public QuizViewModel(int userId, int lessonId)
    {
        _dataAccess = App.DataAccess;
        _userId = userId;
        _lessonId = lessonId;
        LoadQuestionsAsync();
    }

    private async void LoadQuestionsAsync()
    {
        _questions = await _dataAccess.GetQuizQuestionsAsync(_userId, _lessonId, 10);
        _totalQuestions = _questions.Count;
        LoadNextQuestion();
    }

    private void LoadNextQuestion()
    {
        _currentQuestionIndex++;
        if (_currentQuestionIndex >= _questions.Count)
        {
            IsQuizComplete = true;
            UpdateProgressMessage();
            return;
        }

        var question = _questions[_currentQuestionIndex];
        CurrentQuestion = question.Question;
        Answers.Clear();
        foreach (var answer in question.Answers)
        {
            Answers.Add(answer);
        }

        IsAnswerSelected = false;
        SelectedAnswerIndex = -1;
        FeedbackMessage = string.Empty;
        UpdateProgressMessage();
    }

    private void UpdateProgressMessage()
    {
        if (_currentQuestionIndex >= 0 && _currentQuestionIndex < _questions.Count)
        {
            var question = _questions[_currentQuestionIndex];
            var attempts = question.IncorrectAttempts;
            var nextReview = question.NextReviewDate;

            if (attempts > 0)
            {
                ProgressMessage = $"Previous attempts: {attempts}";
                if (nextReview.HasValue && nextReview.Value > DateTime.UtcNow)
                {
                    ProgressMessage += $" | Next review: {nextReview.Value:g}";
                }
            }
            else
            {
                ProgressMessage = "First attempt";
            }
        }
        else
        {
            ProgressMessage = string.Empty;
        }
    }

    [RelayCommand]
    private void SelectAnswer(int index)
    {
        if (IsAnswerSelected) return;
        
        SelectedAnswerIndex = index;
        IsAnswerSelected = true;
        IsCorrect = index == _questions[_currentQuestionIndex].CorrectAnswer;
        
        FeedbackMessage = IsCorrect ? "Correct!" : "Incorrect. Try again!";
        if (IsCorrect)
        {
            Score++;
            _ = RecordAnswerAsync(true);
        }
        else
        {
            _ = RecordAnswerAsync(false);
        }
    }

    [RelayCommand]
    private void NextQuestion()
    {
        if (!IsAnswerSelected) return;
        LoadNextQuestion();
    }

    [RelayCommand]
    private void RestartQuiz()
    {
        _currentQuestionIndex = -1;
        Score = 0;
        IsQuizComplete = false;
        LoadQuestionsAsync();
    }

    private async Task RecordAnswerAsync(bool wasCorrect)
    {
        await _dataAccess.RecordQuizAttemptAsync(_userId, _questions[_currentQuestionIndex].Id, wasCorrect);
    }
} 