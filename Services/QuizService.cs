using Lab3.DataModels;
using Lab3.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Lab3.Services;
using System.Threading.Tasks;
using System.IO;

namespace Lab3.Services
{
    public  class QuizService
    {        
        private readonly DbService _dbService = new DbService();

        public async Task SetupQuizAsync(string quizTitle)
        {
            Global.CurrentQuiz = new Quiz();
            QuizDAO CurrentQuizDAO = await _dbService.ReadData(quizTitle);

            if (CurrentQuizDAO == null)
                throw new ArgumentNullException("QuizDAO is null");
            else
            {
                Global.CurrentQuiz._title = CurrentQuizDAO.Title;
                foreach (Question question in CurrentQuizDAO.Questions)
                {
                    Global.CurrentQuiz.AddQuestion(question.Statement, question.CorrectAnswer, question.Category, question.ImageFileName, question.Answers);
                }
            }
           
        }

        public bool IsValidFilename(string title)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (title.Any(ch => invalidChars.Contains(ch)))
            {
                return false;
            }

            string[] reservedNames = {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

            string titleWithoutExtension = Path.GetFileNameWithoutExtension(title);
            if (reservedNames.Contains(titleWithoutExtension, StringComparer.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (title.Length > 255) // You may need to adjust this for the full path length if necessary
            {
                return false;
            }

            return true;
        }

    }
}
