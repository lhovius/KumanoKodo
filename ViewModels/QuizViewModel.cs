using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace KumanoKodo.ViewModels;

public partial class QuizViewModel : ObservableObject
{
    private readonly IDataAccess _dataAccess;
    private readonly ILogger<QuizViewModel> _logger;

    public ObservableCollection<QuizQuestion> Questions { get; } = new();

    [ObservableProperty]
    private int currentQuestionIndex;

    [ObservableProperty]
    private int score;

    [ObservableProperty]
    private bool isQuizComplete;

    [ObservableProperty]
    private string feedbackMessage = string.Empty;

    public QuizViewModel(IDataAccess dataAccess, ILogger<QuizViewModel> logger)
    {
        _dataAccess = dataAccess;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadQuestionsAsync(int lessonId)
    {
        try
        {
            var questions = await _dataAccess.GetQuizQuestionsAsync(1, lessonId); // TODO: Replace hardcoded user ID
            Questions.Clear();
            foreach (var question in questions)
            {
                Questions.Add(question);
            }
            CurrentQuestionIndex = 0;
            Score = 0;
            IsQuizComplete = false;
            FeedbackMessage = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading quiz questions");
            FeedbackMessage = "Error loading questions. Please try again.";
        }
    }

    [RelayCommand]
    private async Task SubmitAnswerAsync(int answerIndex)
    {
        if (CurrentQuestionIndex >= Questions.Count) return;

        var currentQuestion = Questions[CurrentQuestionIndex];
        bool isCorrect = answerIndex == currentQuestion.CorrectAnswer;

        try
        {
            await _dataAccess.RecordQuizAttemptAsync(1, currentQuestion.Id, isCorrect); // TODO: Replace hardcoded user ID

            if (isCorrect)
            {
                Score++;
                FeedbackMessage = "Correct!";
            }
            else
            {
                FeedbackMessage = $"Incorrect. The correct answer was: {currentQuestion.Answers[currentQuestion.CorrectAnswer]}";
            }

            if (CurrentQuestionIndex < Questions.Count - 1)
            {
                CurrentQuestionIndex++;
            }
            else
            {
                IsQuizComplete = true;
                FeedbackMessage = $"Quiz complete! Your score: {Score}/{Questions.Count}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording quiz attempt");
            FeedbackMessage = "Error recording your answer. Please try again.";
        }
    }

    [RelayCommand]
    private void RestartQuiz()
    {
        CurrentQuestionIndex = 0;
        Score = 0;
        IsQuizComplete = false;
        FeedbackMessage = string.Empty;
    }
} 