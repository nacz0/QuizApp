using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using QuizApp.Model;
using QuizApp.Services;
using QuizApp.ViewModels.BaseClass;

namespace QuizApp.ViewModels
{
    public class SolvingViewModel : ViewModelBase
    {
        private Quiz _currentQuiz;
        private int _currentQuestionIndex;
        private DateTime _startTime;
        private DispatcherTimer _timer;

        public string QuizName { get; private set; }
        public ObservableCollection<QuestionViewModel> Questions { get; } = new();

        public QuestionViewModel CurrentQuestion =>
            (_currentQuestionIndex >= 0 && _currentQuestionIndex < Questions.Count)
                ? Questions[_currentQuestionIndex]
                : null;

        public int CurrentQuestionNumber => _currentQuestionIndex + 1;
        public int TotalQuestions => Questions.Count;

        private TimeSpan _elapsedTime;
        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            private set
            {
                _elapsedTime = value;
                OnPropertyChanged(nameof(ElapsedTime));
                OnPropertyChanged(nameof(FormattedElapsedTime));
            }
        }

        public string FormattedElapsedTime => $"Czas: {ElapsedTime:mm\\:ss}";

        private double _score;
        public double Score
        {
            get => _score;
            private set { _score = value; OnPropertyChanged(nameof(Score)); }
        }

        private bool _isQuizStarted;
        public bool IsQuizStarted
        {
            get => _isQuizStarted;
            private set { _isQuizStarted = value; OnPropertyChanged(nameof(IsQuizStarted)); }
        }

        public ICommand LoadQuizCommand { get; }
        public ICommand StartQuizCommand { get; }
        public ICommand NextQuestionCommand { get; }
        public ICommand PreviousQuestionCommand { get; }
        public ICommand ShowResultsCommand { get; }

        public SolvingViewModel()
        {
            LoadQuizCommand = new RelayCommand(_ => LoadQuiz());
            StartQuizCommand = new RelayCommand(_ => StartQuiz(), _ => CanStartQuiz());
            NextQuestionCommand = new RelayCommand(_ => NextQuestion(), _ => CanMoveNext());
            PreviousQuestionCommand = new RelayCommand(_ => PreviousQuestion(), _ => CanMovePrevious());
            ShowResultsCommand = new RelayCommand(_ => EndQuiz(), _ => CanShowResults());

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) =>
            {
                ElapsedTime = DateTime.Now - _startTime;
            };
        }

        private void LoadQuiz()
        {
            const string password = "SuperSecretPassword123";
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Quiz files (*.quiz)|*.quiz|All files (*.*)|*.*",
                Title = "Wczytaj quiz"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var raw = EncryptionService.DecryptFromFile(dlg.FileName, password);
                var lines = raw.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length == 0 || !lines[0].StartsWith("Quiz: "))
                    throw new InvalidDataException("Nieprawidłowy format pliku.");

                _currentQuiz = new Quiz
                {
                    Title = lines[0].Substring(6).Trim(),
                    Questions = new List<Question>()
                };

                Question cur = null;
                foreach (var line in lines.Skip(1))
                {
                    if (line.StartsWith("Pytanie: "))
                    {
                        if (cur != null) _currentQuiz.Questions.Add(cur);
                        cur = new Question
                        {
                            Text = line.Substring(9).Trim(),
                            Answers = new List<Answer>()
                        };
                    }
                    else if (line.StartsWith("- ") && cur != null)
                    {
                        var idx = line.IndexOf(" (Poprawna:");
                        var text = line.Substring(2, idx - 2).Trim();
                        var isCorrect = line.EndsWith("True)");
                        cur.Answers.Add(new Answer(text, isCorrect));
                    }
                }
                if (cur != null) _currentQuiz.Questions.Add(cur);

                QuizName = _currentQuiz.Title;
                Questions.Clear();
                foreach (var q in _currentQuiz.Questions)
                {
                    Questions.Add(new QuestionViewModel
                    {
                        Text = q.Text,
                        Answers = new ObservableCollection<AnswerViewModel>(
                            q.Answers.Select(a => new AnswerViewModel(a.Text, a.IsCorrect)))
                    });
                }

                _currentQuestionIndex = 0;
                ElapsedTime = TimeSpan.Zero;
                Score = 0;
                IsQuizStarted = false;

                OnPropertyChanged(nameof(QuizName));
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentQuestionNumber));
                OnPropertyChanged(nameof(TotalQuestions));
                OnPropertyChanged(nameof(FormattedElapsedTime));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd wczytywania quizu:\n{ex.Message}",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartQuiz()
        {
            _startTime = DateTime.Now;
            ElapsedTime = TimeSpan.Zero;
            Score = 0;
            IsQuizStarted = true;
            OnPropertyChanged(nameof(FormattedElapsedTime));
            _timer.Start();
        }

        private void NextQuestion()
        {
            if (_currentQuestionIndex < Questions.Count - 1)
            {
                _currentQuestionIndex++;
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentQuestionNumber));
            }
        }

        private void PreviousQuestion()
        {
            if (_currentQuestionIndex > 0)
            {
                _currentQuestionIndex--;
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentQuestionNumber));
            }
        }

        private void EndQuiz()
        {
            _timer.Stop();
            CalculateScore();

            foreach (var question in Questions)
            {
                foreach (var answer in question.Answers)
                {
                    answer.LockAnswer();
                }
            }

            OnPropertyChanged(nameof(FormattedElapsedTime));

            MessageBox.Show(
                $"Koniec quizu!\nWynik: {Score:F2}\n{FormattedElapsedTime}",
                "Wyniki", MessageBoxButton.OK, MessageBoxImage.Information
            );
        }

        private void CalculateScore()
        {
            double total = 0;
            foreach (var qvm in Questions)
            {
                var correctCount = qvm.Answers.Count(a => a.IsCorrect);
                if (correctCount == 0) continue;
                double w = 1.0 / correctCount, s = 0;
                foreach (var a in qvm.Answers)
                    if (a.IsSelected) s += a.IsCorrect ? w : -w;
                total += Math.Max(0, s);
            }
            Score = total;
        }

        private bool CanStartQuiz() => _currentQuiz != null && !IsQuizStarted;
        private bool CanMoveNext() => IsQuizStarted && _currentQuestionIndex < Questions.Count - 1;
        private bool CanMovePrevious() => IsQuizStarted && _currentQuestionIndex > 0;
        private bool CanShowResults() => IsQuizStarted;
    }
}
