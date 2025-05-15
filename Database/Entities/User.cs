namespace TelegramQuiz.Database.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        public List<QuizTest> QuizTests { get; } = new List<QuizTest>();
    }
}
