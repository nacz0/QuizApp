using System.Collections.ObjectModel;
using System.Linq;
using QuizApp.ViewModels;
namespace QuizApp.Helpers
{
    public static class QuizValidator
    {
        public static bool ValidateQuestion(string questionText, ObservableCollection<AnswerViewModel> answers, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(questionText))
            {
                errorMessage = "Tekst pytania nie mo¿e byæ pusty.";
                return false;
            }

            if (answers.Any(a => string.IsNullOrWhiteSpace(a.Text)))
            {
                errorMessage = "Wszystkie odpowiedzi musz¹ mieæ tekst.";
                return false;
            }

            if (!answers.Any(a => a.IsCorrect))
            {
                errorMessage = "Pytanie musi mieæ co najmniej jedn¹ poprawn¹ odpowiedŸ.";
                return false;
            }

            return true;
        }

        public static bool ValidateAllQuestions(ObservableCollection<QuestionViewModel> questions, out string errorMessage)
        {
            foreach (var question in questions)
            {
                if (string.IsNullOrWhiteSpace(question.Text))
                {
                    errorMessage = "Wszystkie pytania musz¹ mieæ tekst.";
                    return false;
                }

                if (question.Answers.Any(a => string.IsNullOrWhiteSpace(a.Text)))
                {
                    errorMessage = $"Pytanie \"{question.Text}\" zawiera odpowiedŸ bez tekstu.";
                    return false;
                }

                if (!question.Answers.Any(a => a.IsCorrect))
                {
                    errorMessage = $"Pytanie \"{question.Text}\" musi mieæ co najmniej jedn¹ poprawn¹ odpowiedŸ.";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
