namespace TelegramQuiz
{
    class Statistics
    {
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }

        public FileWriter FileWriter;

        public Statistics(int correctAnswers, int incorrectAnswers, FileWriter writer)
        {
            CorrectAnswers = correctAnswers;
            IncorrectAnswers = incorrectAnswers;
            FileWriter = writer;
        }

        public string PrintReport()
        {
            return $"Correct Answers: {CorrectAnswers} Incorrect Answers: {IncorrectAnswers} percentage of correct answers: {((float)CorrectAnswers/(CorrectAnswers + IncorrectAnswers))*100} %";
        }
    }
}
