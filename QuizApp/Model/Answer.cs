namespace QuizApp.Model
{
    public class Answer
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }

        public Answer() { }
        public Answer(string text, bool isCorrect)
        {
            Text = text;
            IsCorrect = isCorrect;
        }

        public override string ToString() => Text;
    }
}
