using Lab3.DataModels;
using Lab3.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Lab3.Services;
using System.Threading.Tasks;

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
    }
}
