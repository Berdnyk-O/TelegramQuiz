using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TelegramQuiz
{
    public class QuestionData
    {
        private const string QuestionsYmlDirPath = "D:/Projects/TelegramQuiz/Yml/questions.yaml";
        private const string QuestionsJsonDirPath = "D:/Projects/TelegramQuiz/Json/questions.json";
        private const string TestFileExtension = "*.yaml";

        public List<Question> Questions { get; set; }
        public List<Thread> Threads { get; set; }

        public QuestionData(Question[] questions)
        {
            Questions = questions.ToList();
            Threads = new List<Thread>(Questions.Count);
        }

        public QuestionData() 
        {
            Questions = new List<Question>(2);
            Threads = new List<Thread>(2);
            LoadData();
        }

        public string[] ToYaml()
        {
            string[] questinsYaml = new string[Questions.Count];

            for (int i = 0; i < questinsYaml.Length; i++)
            {
                questinsYaml[i] = Questions[i].ToYaml();
            }

            return questinsYaml;
        }

        public bool SaveToYaml(string[] questinsYaml)
        {
            string yamlContent = string.Join(Environment.NewLine, questinsYaml);
            
            File.WriteAllText(QuestionsYmlDirPath, yamlContent);

            return true;
        }

        public string[] ToJson()
        {
            string[] questinsJson = new string[Questions.Count];

            for (int i = 0; i < questinsJson.Length; i++)
            {
                questinsJson[i] = Questions[i].ToJson();
            }

            return questinsJson;
        }

        public bool SaveToJson(string[] questinsJson)
        {
            string yamlContent = string.Join(Environment.NewLine, questinsJson);

            File.WriteAllText(QuestionsJsonDirPath, yamlContent);

            return true;
        }

        public string PrepareFilename(string fileName)
        {
            return Path.Combine(Environment.CurrentDirectory, fileName);
        }

        public void EachFile(string directory, string searchPattern, Action<string> action)
        {
            var files = Directory.GetFiles(directory, searchPattern);

            foreach (var file in files)
            {
                action(file);
            }
        }

        public void InThread(Action block)
        {
            Threads.Add(new Thread(new ThreadStart(block)));
            Threads[^1].Start();
        }

        public void LoadFromFile(string fileName)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            using var reader = new StreamReader(fileName);
            var questions = deserializer.Deserialize<List<Question>>(reader);

            Questions.AddRange(questions);
        }

        public void ShuffleAnswers()
        {
            foreach (var question in Questions)
            {
                question.ToHash();
                question.Shuffle(question.Answers);
            }
        }

        public void LoadData()
        {
            string baseDir = AppContext.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\Yml"));

            EachFile(projectRoot,
                TestFileExtension,
                (str) => 
                {
                    var file = PrepareFilename(str);
                    /*InThread(()=>LoadFromFile(file));*/
                    LoadFromFile(file);
                });

            ShuffleAnswers();
        }
    }
}
