namespace TelegramQuiz.Database.Entities
{
    public class QuizTest
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public decimal Percent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; }
        public int UserId { get; set; }
        
    }
}
