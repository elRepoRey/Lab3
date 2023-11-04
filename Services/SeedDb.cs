using System;
using System.Collections.Generic;
using System.IO;
using Lab3.DataModels;
using Newtonsoft.Json;

namespace Lab3.Services
{
    public class SeedDb
    {
        private const string SeedDataPath = "QuizSeedData.json";
        private readonly string _baseDirectory;

        public SeedDb()
        {
            _baseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyQuiz");

            if (!Directory.Exists(_baseDirectory))
            {
                Directory.CreateDirectory(_baseDirectory);
            }
        }

        public void CreateFilesFromSeed()
        {
            if (File.Exists(SeedDataPath))
            {
                using (StreamReader reader = new StreamReader(SeedDataPath))
                {
                    string json = reader.ReadToEnd();
                    var quizzes = JsonConvert.DeserializeObject<List<QuizDAO>>(json);

                    foreach (var quiz in quizzes)
                    {
                        var sanitizedTitle = SanitizeFileName(quiz.Title);
                        var quizFilePath = Path.Combine(_baseDirectory, sanitizedTitle + ".json");

                        if (!File.Exists(quizFilePath))
                        {
                            var dbService = new DbService();
                            dbService.WriteData(quiz, quizFilePath);
                        }
                    }
                }
            }
        }

        private string SanitizeFileName(string filename)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }
            return filename;
        }
    }
}
