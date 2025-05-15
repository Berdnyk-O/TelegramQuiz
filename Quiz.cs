namespace TelegramQuiz
{
    class Quiz
    {
        public string YmlDirPath { get; private set; } = "D:/Projects/TelegramQuiz/Yml/questions.yaml";
        public string AnswersDirPath { get; private set; } = "D:/Projects/TelegramQuiz/Json/questions.json";
        public string FileExtension { get; private set; } = ".yaml";


        public void Config(string ymlDirPath, string answersDirPath, string fileExtension)
        {
            YmlDirPath = ymlDirPath;
            AnswersDirPath = answersDirPath;
            FileExtension = fileExtension;
        }
    }
}
