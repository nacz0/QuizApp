using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.ViewModels
{
    public class QuestionViewModel : ViewModelBase
    {
        public string Text { get; set; }
        public ObservableCollection<AnswerViewModel> Answers { get; set; }
    }
}
