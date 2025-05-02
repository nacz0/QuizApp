   namespace QuizApp.Model
   {
       class QuizResult
       {
           public Quiz Quiz { get; set; }
           public int TotalQuestions => Quiz.Questions.Count;
           public int CorrectAnswers { get; set; }
           public int IncorrectAnswers => TotalQuestions - CorrectAnswers;

           public QuizResult(Quiz quiz, int correctAnswers)
           {
               Quiz = quiz;
               CorrectAnswers = correctAnswers;
           }
       }
   }
   