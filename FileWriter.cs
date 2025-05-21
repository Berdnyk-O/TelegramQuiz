namespace TelegramQuiz
{
    public class FileWriter
    {
        public string AnswersDir { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }

        public FileWriter(string answersDir, string fileName, string fileExtension)
        {
            AnswersDir = answersDir;
            FileName = fileName;
            FileExtension = fileExtension;
        }

        public void Write(string message)
        {
            var path = PrepareFileName(FileName);
            File.AppendAllText(PrepareFileName(path), message);
        }

        public string PrepareFileName(string fileName)
        {
            string baseDir = AppContext.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @$"..\..\..\{AnswersDir}"));
            
            return Path.Combine(projectRoot, $"{fileName}{FileExtension}");
        }
    }
}
