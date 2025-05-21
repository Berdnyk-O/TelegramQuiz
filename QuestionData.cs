using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TelegramQuiz
{
    public class QuestionData
    {
        private const string QuestionsYmlDirPath = "D:/Projects/TelegramQuiz/Yml";
        private const string QuestionsJsonDirPath = "D:/Projects/TelegramQuiz/Json";
        private const string TestFileExtension = "*.yaml";

        public List<Question> Questions { get; set; }
        public List<Thread> Threads { get; set; }

        private readonly object _lock = new object();

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

        public bool SaveToYaml(string[] questionsYaml)
        {
            string yamlContent = string.Join(Environment.NewLine, questionsYaml);
            string filePath = Path.Combine(QuestionsYmlDirPath, "questions.yaml");

            Directory.CreateDirectory(QuestionsJsonDirPath);
            File.WriteAllText(filePath, yamlContent);

           /* var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
            string filePath = Path.Combine(QuestionsYmlDirPath, "questions.yaml");

            Directory.CreateDirectory(QuestionsJsonDirPath);
            File.WriteAllText(filePath, serializer.Serialize(Questions));*/
             
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
            string jsonContent = "[" + string.Join(",", questinsJson) + "]";
            string filePath = Path.Combine(QuestionsJsonDirPath, "questions.json");

            Directory.CreateDirectory(QuestionsJsonDirPath);
            File.WriteAllText(filePath, jsonContent);

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
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            List<Question> questions;

            using var reader = new StreamReader(fileName);

            if (extension == ".yaml" || extension == ".yml")
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                questions = deserializer.Deserialize<List<Question>>(reader);
            }
            else if (extension == ".json")
            {
                var content = reader.ReadToEnd();
                questions = JsonSerializer.Deserialize<List<Question>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
            }
            else
            {
                throw new NotSupportedException($"Формат файлу '{extension}' не підтримується.");
            }

            lock (_lock)
            {
                Questions.AddRange(questions);
            }
        }

        public void ShuffleAnswers()
        {
            foreach (var question in Questions)
            {
                question.Shuffle(question.Answers);
                question.ToHash();
            }
        }

        public void LoadData()
        {
            string extension = TestFileExtension.Substring(2);
            string projectRoot = QuestionsYmlDirPath;

            if (extension == "json")
            {
                projectRoot = QuestionsJsonDirPath;
            }

            EachFile(projectRoot,
                TestFileExtension,
                (str) =>
                {
                    var file = PrepareFilename(str);
                    InThread(() => LoadFromFile(file));
                });

            foreach (var thread in Threads)
            {
                thread.Join();
            }

            ShuffleAnswers();
        }
    }
}
