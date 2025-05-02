using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
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
            }
        }

        public QuestionViewModel SelectedQuestion
        {
            get => _selectedQuestion;
            set
            {
                _selectedQuestion = value;
                OnPropertyChanged(nameof(SelectedQuestion));
                // Aktualizuj tekst pytania i odpowiedzi w edytorze
                if (_selectedQuestion != null)
                {
                    QuestionText = _selectedQuestion.Text;
                    Answers.Clear();
                    foreach (var answer in _selectedQuestion.Answers)
                    {
                        Answers.Add(new AnswerViewModel
                        {
                            Text = answer.Text,
                            IsCorrect = answer.IsCorrect
                        });
                    }
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

        public MakerViewModel()
        {
            Answers = new ObservableCollection<AnswerViewModel>();
            Questions = new ObservableCollection<QuestionViewModel>();

            AddAnswerCommand = new RelayCommand(_ => AddAnswer(), _ => true);
            NextQuestionCommand = new RelayCommand(_ => AddQuestion(), _ => CanAddQuestion());
            FinishQuizCommand = new RelayCommand(_ => FinishQuiz(), _ => CanFinishQuiz());
            LoadQuizCommand = new RelayCommand(_ => LoadQuiz(), _ => true);
            EditQuestionCommand = new RelayCommand(_ => EditQuestion(), _ => SelectedQuestion != null);
            DeleteQuestionCommand = new RelayCommand(_ => DeleteQuestion(), _ => SelectedQuestion != null);
        }

        private void AddAnswer()
        {
            Answers.Add(new AnswerViewModel());
        }

        private void AddQuestion()
        {
            Questions.Add(new QuestionViewModel
            {
                Text = QuestionText,
                Answers = new ObservableCollection<AnswerViewModel>(Answers)
            });

            QuestionText = string.Empty;
            Answers.Clear();
        }

        private void EditQuestion()
        {
            if (SelectedQuestion != null)
            {
                SelectedQuestion.Text = QuestionText;
                SelectedQuestion.Answers.Clear();
                foreach (var answer in Answers)
                {
                    SelectedQuestion.Answers.Add(new AnswerViewModel
                    {
                        Text = answer.Text,
                        IsCorrect = answer.IsCorrect
                    });
                }

                // Resetuj edytor
                QuestionText = string.Empty;
                Answers.Clear();
                SelectedQuestion = null;
            }
        }

        private void DeleteQuestion()
        {
            if (SelectedQuestion != null)
            {
                Questions.Remove(SelectedQuestion);
                SelectedQuestion = null;

                // Resetuj edytor
                QuestionText = string.Empty;
                Answers.Clear();
            }
        }

        private bool CanAddQuestion()
        {
            return !string.IsNullOrWhiteSpace(QuestionText) && Answers.Any() && Answers.Any(a => a.IsCorrect);
        }

        private void FinishQuiz()
        {
            if (string.IsNullOrWhiteSpace(QuizName))
                throw new InvalidOperationException("Nazwa quizu nie może być pusta.");

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

            QuizName = string.Empty;
            Questions.Clear();
            QuestionText = string.Empty;
            Answers.Clear();
        }

        private bool CanFinishQuiz()
        {
            return !string.IsNullOrWhiteSpace(QuizName) && Questions.Any();
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

                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Wybrany plik nie istnieje.");

                var decryptedData = EncryptionService.DecryptFromFile(filePath, password);

                var lines = decryptedData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0 || !lines[0].StartsWith("Quiz: "))
                    throw new InvalidDataException("Plik quizu ma nieprawidłowy format.");

                QuizName = lines[0].Replace("Quiz: ", string.Empty).Trim();
                Questions.Clear();

                QuestionViewModel currentQuestion = null;

                foreach (var line in lines.Skip(1))
                {
                    if (line.StartsWith("Pytanie: "))
                    {
                        if (currentQuestion != null)
                            Questions.Add(currentQuestion);

                        currentQuestion = new QuestionViewModel
                        {
                            Text = line.Replace("Pytanie: ", string.Empty).Trim(),
                            Answers = new ObservableCollection<AnswerViewModel>()
                        };
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
        }
    }

    public class QuestionViewModel
    {
        public string Text { get; set; }
        public ObservableCollection<AnswerViewModel> Answers { get; set; }
    }

    public class AnswerViewModel : ViewModelBase
    {
        private string _text = "";
        private bool _isCorrect;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public bool IsCorrect
        {
            get => _isCorrect;
            set
            {
                _isCorrect = value;
                OnPropertyChanged(nameof(IsCorrect));
            }
        }
    }
}
