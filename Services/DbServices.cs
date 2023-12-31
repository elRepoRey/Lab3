﻿using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Lab3.DataModels;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Documents;
using System.Windows;

namespace Lab3.Services
{
    public class DbService
    {
        private readonly string MainDirectoryPath;
        private const string SeedDataPath = "QuizSeedData.json";
        private const string SourcePlaceholderImageName = "PlaceholderImage.png";
        private const string ImageDirectoryName = "Images";

        public DbService()
        {
            MainDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyQuiz");
            if (!Directory.Exists(MainDirectoryPath))
            {
                Directory.CreateDirectory(MainDirectoryPath);
            }

            string imagesDirectoryPath = Path.Combine(MainDirectoryPath, "Images");
            if (!Directory.Exists(imagesDirectoryPath))
            {
                Directory.CreateDirectory(imagesDirectoryPath);

                SeedPlaceholderImage();
            }

        }

        private string? GetFilePath(string title)
        {
            try 
            { 
            string fileName = $"{title}.json";
            return Path.Combine(MainDirectoryPath, fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public async Task<QuizDAO?> ReadData(string title)
        {
            try
            { 
            string filePath = GetFilePath(title);

            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string json = await reader.ReadToEndAsync();
                    QuizDAO quiz = JsonConvert.DeserializeObject<QuizDAO>(json);
                    return quiz;

                }
            }
            return default;
            }
            catch (Exception ex)
            {
               throw new Exception(ex.Message);
            }
        }


        public async Task WriteData(QuizDAO data, string title)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                string filePath = GetFilePath(title);

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    await writer.WriteAsync(json);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async IAsyncEnumerable<List<Question>> GetAllQuestionsAsync()
        {
           
            var files = Directory.GetFiles(MainDirectoryPath, "*.json");

            foreach (var file in files)
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    string json = await reader.ReadToEndAsync();
                    var quiz = JsonConvert.DeserializeObject<QuizDAO>(json);

                    if (quiz != null)
                    {
                        yield return quiz.Questions;
                    }
                }
            }
           
        }


        public List<string>? GetAllQuizTitles()
        {
            try
            { 
            var files = Directory.GetFiles(MainDirectoryPath, "*.json");
            var titles = new List<string>();

            foreach (var file in files)
            {
                titles.Add(Path.GetFileNameWithoutExtension(file));
            }

            return titles;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SeedFromData()
        {
            try
            {             
            if (!Directory.EnumerateFiles(MainDirectoryPath, "*.json").Any())
            {
                if (File.Exists(SeedDataPath))
                {
                    using (StreamReader reader = new StreamReader(SeedDataPath))
                    {
                        string json = await reader.ReadToEndAsync();
                        var quizzes = JsonConvert.DeserializeObject<List<QuizDAO>>(json);

                        if (quizzes != null)
                        {
                            foreach (var quiz in quizzes)
                            {
                                var sanitizedTitle = SanitizeFileName(quiz.Title);
                                var quizFilePath = Path.Combine(MainDirectoryPath, sanitizedTitle + ".json");

                                if (!File.Exists(quizFilePath))
                                {
                                    await WriteData(quiz, sanitizedTitle);
                                }
                            }
                        }
                    }
                }
            }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string? SanitizeFileName(string title)
        {
            try
            { 
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                title = title.Replace(c, '_');
            }
            return title;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void DeleteData(string title)
        {
            try
            { 
            var sanitizedTitle = SanitizeFileName(title);
            string filePath = GetFilePath(sanitizedTitle);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GetImageFolderPath()
        {
            return Path.Combine(MainDirectoryPath, ImageDirectoryName);
        }

        private void SeedPlaceholderImage()
        {           
            
            string destinationImagePath = Path.Combine(GetImageFolderPath(), SourcePlaceholderImageName);

            try
            {               
                if (!File.Exists(SourcePlaceholderImageName))
                {
                    
                    throw new FileNotFoundException($"Source file {SourcePlaceholderImageName} not found.");
                }
             

                File.Copy(SourcePlaceholderImageName, destinationImagePath);
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Error seeding placeholder image: {ex.Message}");
                return;
            }
        }

        public void CopyFile(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                if (!File.Exists(destinationFilePath))
                {
                    File.Copy(sourceFilePath, destinationFilePath);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
