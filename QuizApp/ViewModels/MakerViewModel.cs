using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using QuizApp.Helpers;
using QuizApp.Services;
using QuizApp.ViewModels.BaseClass;

namespace QuizApp.ViewModels
{
    public class MakerViewModel : ViewModelBase
    {
        private string _quizName;
        private string _questionText;
        private QuestionViewModel _selectedQuestion;

        public string QuizName
        {
            get => _quizName;
            set
            {
                _quizName = value;
                OnPropertyChanged(nameof(QuizName));
            }
        }

        public string QuestionText
        {
            get => _questionText;
            set
            {
                _questionText = value;
                OnPropertyChanged(nameof(QuestionText));
                CommandManager.InvalidateRequerySuggested(); // Odśwież stan komend
            }
        }

        public QuestionViewModel SelectedQuestion
        {
            get => _selectedQuestion;
            set
            {
                _selectedQuestion = value;
                OnPropertyChanged(nameof(SelectedQuestion));

                if (_selectedQuestion != null)
                {
                    QuestionText = _selectedQuestion.Text;
                    QuestionService.UpdateAnswers(_selectedQuestion.Answers, Answers);
                }
            }
        }

        public ObservableCollection<AnswerViewModel> Answers { get; }
        public ObservableCollection<QuestionViewModel> Questions { get; }

        public ICommand AddAnswerCommand { get; }
        public ICommand NextQuestionCommand { get; }
        public ICommand FinishQuizCommand { get; }
        public ICommand LoadQuizCommand { get; }
        public ICommand EditQuestionCommand { get; }
        public ICommand DeleteQuestionCommand { get; }
        public ICommand CancelEditCommand { get; }

        public MakerViewModel()
        {
            Answers = new ObservableCollection<AnswerViewModel>();
            ResetAnswers(); // Ustaw domyślne odpowiedzi
            Questions = new ObservableCollection<QuestionViewModel>();

            NextQuestionCommand = new RelayCommand(_ => AddQuestion(), _ => CanAddQuestion());
            FinishQuizCommand = new RelayCommand(_ => FinishQuiz(), _ => CanFinishQuiz());
            LoadQuizCommand = new RelayCommand(_ => LoadQuiz(), _ => true);
            EditQuestionCommand = new RelayCommand(_ => EditQuestion(), _ => SelectedQuestion != null);
            DeleteQuestionCommand = new RelayCommand(_ => DeleteQuestion(), _ => SelectedQuestion != null);
            CancelEditCommand = new RelayCommand(_ => CancelEdit(), _ => SelectedQuestion != null);
        }

        private void CancelEdit()
        {
            if (SelectedQuestion != null)
            {
                ClearEditor();
            }
        }

        private void AddQuestion()
        {
            if (!QuizValidator.ValidateQuestion(QuestionText, Answers, out var errorMessage))
            {
                MessageBox.Show(errorMessage, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Questions.Add(QuestionService.CreateQuestion(QuestionText, Answers));
            ClearEditor();
        }

        private void EditQuestion()
        {
            if (SelectedQuestion != null)
            {
                if (!QuizValidator.ValidateQuestion(QuestionText, Answers, out var errorMessage))
                {
                    MessageBox.Show(errorMessage, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Aktualizuj tekst pytania
                SelectedQuestion.Text = QuestionText;

                // Aktualizuj odpowiedzi
                QuestionService.UpdateAnswers(Answers, SelectedQuestion.Answers);

                // Przejdź do trybu tworzenia nowego pytania
                ClearEditor();
            }
        }

        private void DeleteQuestion()
        {
            if (SelectedQuestion != null)
            {
                Questions.Remove(SelectedQuestion);
                ClearEditor();
            }
        }

        private void FinishQuiz()
        {
            if (!QuizValidator.ValidateAllQuestions(Questions, out var errorMessage))
            {
                MessageBox.Show(errorMessage, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var quizData = new StringBuilder();
            quizData.AppendLine($"Quiz: {QuizName}");
            foreach (var question in Questions)
            {
                quizData.AppendLine($"Pytanie: {question.Text}");
                foreach (var answer in question.Answers)
                {
                    quizData.AppendLine($"- {answer.Text} (Poprawna: {answer.IsCorrect})");
                }
            }

            var filePath = $"{QuizName}.quiz";
            const string password = "SuperSecretPassword123";

            EncryptionService.EncryptToFile(filePath, quizData.ToString(), password);

            // Resetuj nazwę quizu
            QuizName = string.Empty;

            // Wyczyść edytor i pytania
            ClearEditor();
            Questions.Clear();
        }

        private void LoadQuiz()
        {
            const string password = "SuperSecretPassword123";

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Pliki quizu (*.quiz)|*.quiz|Wszystkie pliki (*.*)|*.*",
                Title = "Wybierz plik quizu"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                try
                {
                    if (!File.Exists(filePath))
                        throw new FileNotFoundException("Wybrany plik nie istnieje.");

                    var decryptedData = EncryptionService.DecryptFromFile(filePath, password);

                    var lines = decryptedData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length == 0 || !lines[0].StartsWith("Quiz: "))
                        throw new InvalidDataException("Plik quizu ma nieprawidłowy format.");

                    QuizName = lines[0].Replace("Quiz: ", string.Empty).Trim();
                    Questions.Clear();
                    ResetAnswers();

                    QuestionViewModel currentQuestion = null;

                    foreach (var line in lines.Skip(1))
                    {
                        if (line.StartsWith("Pytanie: "))
                        {
                            if (currentQuestion != null)
                                Questions.Add(currentQuestion);

                            currentQuestion = QuestionService.CreateQuestion(line.Replace("Pytanie: ", string.Empty).Trim(), new ObservableCollection<AnswerViewModel>());
                        }
                        else if (line.StartsWith("- "))
                        {
                            if (currentQuestion == null)
                                throw new InvalidDataException("Odpowiedź bez pytania w pliku quizu.");

                            var answerText = line.Substring(2, line.LastIndexOf(" (Poprawna: ") - 2).Trim();
                            var isCorrect = line.Contains("(Poprawna: True)");

                            currentQuestion.Answers.Add(new AnswerViewModel
                            {
                                Text = answerText,
                                IsCorrect = isCorrect
                            });
                        }
                    }

                    if (currentQuestion != null)
                        Questions.Add(currentQuestion);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Wystąpił błąd podczas ładowania quizu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetAnswers()
        {
            Answers.Clear();
            for (int i = 0; i < 4; i++) // Domyślnie 4 odpowiedzi
            {
                Answers.Add(new AnswerViewModel());
            }
        }

        private void ClearEditor()
        {
            QuestionText = string.Empty;
            ResetAnswers();
            SelectedQuestion = null;
        }

        private bool CanAddQuestion()
        {
            return !string.IsNullOrWhiteSpace(QuestionText)
                   && Answers.Any()
                   && Answers.All(a => !string.IsNullOrWhiteSpace(a.Text))
                   && Answers.Any(a => a.IsCorrect)
                   && SelectedQuestion == null;
        }

        private bool CanFinishQuiz()
        {
            return !string.IsNullOrWhiteSpace(QuizName) && Questions.Any();
        }
    }

}
