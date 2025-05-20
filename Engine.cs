using Microsoft.EntityFrameworkCore;
using TelegramQuiz.Database;
using TelegramQuiz.Database.Entities;

namespace TelegramQuiz
{
    public class Engine
    {
        public QuestionData QuestionData { get; private set; }
        public DateTime CurrentTime { get => DateTime.Now; }
        public FileWriter FileWriter { get; private set; }
        public QuizDbContext Context { get; private set; }
        public Statistics Statistics { get; private set; }
        public string UserName { get; set; }

        public Func<long, string, string[], Task> SendQuestionAsync;

        public Func<long, string, Task> SendMessageAsync;

        private int questionIndex = -1;
        private bool isTest = false;
        private DateTime TestingStartTime;

        public Engine(QuestionData questionData,
            FileWriter fileWriter,
            QuizDbContext context,
            Statistics statistics)
        {
            QuestionData = questionData;
            FileWriter = fileWriter;
            Context = context;
            Statistics = statistics;
        }

        public async Task Run(long chatId)
        {
            if (isTest == false)
            {
                questionIndex = -1;
            }

            questionIndex++;
            
            if (questionIndex < QuestionData.Questions.Count)
            {
                isTest = true;
                
                if (questionIndex < 1)
                {
                    TestingStartTime = DateTime.Now;
                }

                await SendQuestionAsync!(chatId,
                    QuestionData.Questions[questionIndex].Body,
                    QuestionData.Questions[questionIndex].DisplayAnswers());
            }
            else
            {
                await SendMessageAsync!(chatId, $"Вітаю, ви пройшли Quiz");
                isTest = false;
                await EndQuiz(chatId, true);
            }
        }

        public async Task StopTest(long chatId)
        {
            isTest = false;

            await SendMessageAsync!(chatId, $"Quiz зупинено");
            await EndQuiz(chatId, false);
        }

        public async Task CheckAsync(long chatId, string userAnswerLetter)
        {
            var userAnswer = QuestionData.Questions[questionIndex].FindAnswerByChar(userAnswerLetter);
            
            if (userAnswer == QuestionData.Questions[questionIndex].CorrectAnswer)
            {
                await SendMessageAsync!(chatId, $"Вірно");
                
                if (isTest)
                {
                    Statistics.CorrectAnswers++;
                }
                    
            }
            else
            {
                await SendMessageAsync!(chatId, $"Помилка");

                if (isTest)
                {
                    Statistics.IncorrectAnswers++;
                }
            }

            if (isTest)
            {
                await Run(chatId);
            }
        }

        public async Task GetQuestion(long chatId, int index)
        {
            if (isTest)
            {
                await SendMessageAsync!(chatId, $"Спочатку пройдіть тест");
                return;
            }

            if (index >= 0 && index < QuestionData.Questions.Count)
            {
                questionIndex = index;
                await SendQuestionAsync!(chatId,
                   QuestionData.Questions[questionIndex].Body,
                   QuestionData.Questions[questionIndex].DisplayAnswers());
            }
            else
            {
                await SendMessageAsync!(chatId, $"Немає запитання під таким номером");
            }
            
        }

        private async Task EndQuiz(long chatId, bool isComplete)
        {
            await SendMessageAsync!(chatId, Statistics.PrintReport());

            FileWriter.FileName = UserName;
            FileWriter.Write($"{TestingStartTime} - {Statistics.PrintReport()}\n");

            if (!await Context.Users.AnyAsync(x => x.UserName == UserName))
            {
                var user = new User
                {
                    UserName = UserName,
                };

                await Context.Users.AddAsync(user);
                await Context.SaveChangesAsync();
            }

            var quizTest = new QuizTest
            {
                Status = isComplete ? 1 : 0,
                CorrectAnswers = Statistics.CorrectAnswers,
                IncorrectAnswers = Statistics.IncorrectAnswers,
                Percent = (decimal)Statistics.Percent,
                CreatedAt = TestingStartTime.ToUniversalTime(),
                UpdatedAt = TestingStartTime.ToUniversalTime(),
                UserId = Context.Users.FirstOrDefault(x => x.UserName == UserName)!.Id
            };

            await Context.Tests.AddAsync(quizTest);
            await Context.SaveChangesAsync();

            questionIndex = -1;
            Statistics.CorrectAnswers = 0;
            Statistics.IncorrectAnswers = 0;
        }
    }
}
