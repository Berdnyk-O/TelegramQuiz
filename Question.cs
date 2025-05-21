using Newtonsoft.Json;
using System.Collections;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TelegramQuiz
{
    public sealed class Question
    {
        public string Body { get; set; }
        public string[] Answers { get; set; }
        public string CorrectAnswer { get; set; }

        private Hashtable _hashtable;
        
        private static string[] _answerLetters => new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I" };

        public Question() { }

        public Question(string body, string[] answears, string correctAnswer)
        {
            Body = body;
            Answers = answears;
            CorrectAnswer = correctAnswer;
        }

        public string[] DisplayAnswers()
        {
            string[] formattedAnswers = new string[Answers.Length];

            for (int i = 0; i < formattedAnswers.Length; i++)
            {
                formattedAnswers[i] = $"{_answerLetters[i]}.{Answers[i]}";
            }

            return formattedAnswers;
        }

        public Hashtable ToHash()
        {
            _hashtable = new();

            for (int i = 0; i < Answers.Length; i++)
            {
                _hashtable.Add(_answerLetters[i], Answers[i]);
            }

            return _hashtable;
        }

        public string? FindAnswerByChar(string answerChar)
        {
            return _hashtable[answerChar]?.ToString();
        }

        public Hashtable LoadAnswers(string[] answers)
        {
            Shuffle(answers);
            return ToHash(answers);
        }

        public void Shuffle(string[] array)
        {
            var random = new Random();

            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]); 
            }
        }

        private Hashtable ToHash(string[] answers)
        {
            _hashtable = new();

            for (int i = 0; i < answers.Length; i++)
            {
                _hashtable.Add(_answerLetters[i], answers[i]);
            }

            return _hashtable;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public string ToYaml()
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return serializer.Serialize(this);
        }
    }
}
