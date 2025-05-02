using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Model
{
    class Quiz
    {
        public string Title { get; set; }

        public string Description { get; set; }
        public List<Question> Questions { get; set; }
        public Quiz(string title, List<Question> questions)
        {
            Title = title;
            Questions = questions;
        }
        public Quiz()
        {
            Title = string.Empty;
            Questions = new List<Question>();
        }

    }
}
