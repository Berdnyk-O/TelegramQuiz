namespace TelegramQuiz
{
    public class Statistics
    {
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public float Percent { get; set; }

        public FileWriter FileWriter;

        public Statistics(int correctAnswers, int incorrectAnswers, FileWriter writer)
        {
            CorrectAnswers = correctAnswers;
            IncorrectAnswers = incorrectAnswers;
            FileWriter = writer;
        }

        public string PrintReport()
        {
            Percent = ((float)CorrectAnswers / (CorrectAnswers + IncorrectAnswers)) * 100;
            return $"Correct Answers: {CorrectAnswers} Incorrect Answers: {IncorrectAnswers} percentage of correct answers: {Percent} %";
        }
    }
}
