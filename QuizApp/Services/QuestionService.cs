using System.Collections.ObjectModel;
using System.Linq;
using QuizApp.ViewModels;
namespace QuizApp.Services
{
    public static class QuestionService
    {
        public static QuestionViewModel CreateQuestion(string text, ObservableCollection<AnswerViewModel> answers)
        {
            return new QuestionViewModel
            {
                Text = text,
                Answers = new ObservableCollection<AnswerViewModel>(answers.Select(a => new AnswerViewModel
                {
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }))
            };
        }

        public static void UpdateAnswers(ObservableCollection<AnswerViewModel> source, ObservableCollection<AnswerViewModel> target)
        {
            target.Clear();
            foreach (var answer in source)
            {
                target.Add(new AnswerViewModel
                {
                    Text = answer.Text,
                    IsCorrect = answer.IsCorrect
                });
            }
        }
    }
}
