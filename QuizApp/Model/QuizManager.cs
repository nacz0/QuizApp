using System.Collections.Generic;

namespace QuizApp.Model
{
    class QuizManager
    {
        public Quiz CurrentQuiz { get; private set; }
        public int CurrentQuestionIndex { get; private set; }
        public List<Question> Questions => CurrentQuiz.Questions;

        public QuizManager(Quiz quiz)
        {
            CurrentQuiz = quiz;
            CurrentQuestionIndex = 0;
        }

        public Question GetCurrentQuestion()
        {
            return Questions[CurrentQuestionIndex];
        }

        public bool SubmitAnswer(int answerIndex)
        {
            var question = GetCurrentQuestion();
            var selectedAnswer = question.Answers[answerIndex];
            CurrentQuestionIndex++;
            return selectedAnswer.IsCorrect;
        }

        public bool HasMoreQuestions()
        {
            return CurrentQuestionIndex < Questions.Count;
        }
    }
}
