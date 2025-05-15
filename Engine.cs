namespace TelegramQuiz
{
    class Engine
    {
        public QuestionData QuestionData { get; private set; }
        public TelegramClient TelegramClient { get; private set; }
        public DateTime CurrentTime { get => DateTime.Now; }
        public FileWriter FileWriter { get; private set; }
        public Statistics Statistics { get; private set; }
        
        private int questionIndex = -1;
        private DateTime TestingStartTime;

        public Engine(QuestionData questionData,
            TelegramClient telegramClient,
            FileWriter fileWriter,
            Statistics statistics)
        {
            QuestionData = questionData;
            TelegramClient = telegramClient;
            FileWriter = fileWriter;
            Statistics = statistics;

            TelegramClient.OnQuizSelected += async () => await Run();
        }

        public async Task Run()
        {
            questionIndex++;

            if (questionIndex < QuestionData.Questions.Count)
            {
                if(questionIndex < 1) TestingStartTime = DateTime.Now;
                await TelegramClient.SendQuestion(
                    QuestionData.Questions[questionIndex].Body,
                    QuestionData.Questions[questionIndex].DisplayAnswers(),
                    CheckAsync);
            }
            else
            {
                await TelegramClient.SendMessage($"Вітаю, ви пройшли Quiz");
                await TelegramClient.SendMessage(Statistics.PrintReport());

                FileWriter.FileName = TelegramClient.UserName;
                FileWriter.Write($"{TestingStartTime} - {Statistics.PrintReport()}\n");

                questionIndex = -1;
                Statistics.CorrectAnswers = 0;
                Statistics.IncorrectAnswers = 0;
            }
        }

        public async Task CheckAsync(string userAnswerLetter)
        {
            var userAnswer = QuestionData.Questions[questionIndex].FindAnswerByChar(userAnswerLetter);
            
            if (userAnswer == QuestionData.Questions[questionIndex].CorrectAnswer)
            {
                await TelegramClient.SendMessage($"Вірно");
                Statistics.CorrectAnswers++;
            }
            else
            {
                await TelegramClient.SendMessage($"Помилка");
                Statistics.IncorrectAnswers++;
            }

            await Run();
        }
    }
}
