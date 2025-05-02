using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Model
{
    class Answer
    {
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
    public Answer(string text, bool isCorrect)
    {
        Text = text;
        IsCorrect = isCorrect;
    }
    public Answer()

    {
    }
}
}
