using GalaSoft.MvvmLight.Command;
using Lab3.DataModels;
using Lab3.Services;
using Lab3.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Lab3.View
{
    public class CreateQuizViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string ImageTargetFolder { get; private set; }

        private string ImageFileName;
        public Quiz CurrentQuiz { get; private set; }
        public Question CurrentQuestion;      
        public event Action QuizCreatedCompleted;
        private readonly DbService _dbService;
        public bool IsNextQuestionAvailable => CurrentQuestionIndex < CurrentQuiz.Questions.Count() - 1;
        public bool IsPreviousQuestionAvailable => CurrentQuestionIndex > 0;
        public string CurrentQuestionNumberText => $"{CurrentQuestionIndex + 1 } of {CurrentQuiz.Questions.Count() + 1}";
        private int _currentQuestionIndex = 0;
        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            set
            {
                _currentQuestionIndex = value;
                OnPropertyChanged(nameof(CurrentQuestionIndex));
                OnPropertyChanged(nameof(IsNextQuestionAvailable));
                OnPropertyChanged(nameof(IsPreviousQuestionAvailable));
                OnPropertyChanged(nameof(CurrentQuestionNumberText));
            }
        }

        private string _quizTitle;

        public string QuizTitle
        {
            get => _quizTitle;
            set
            {
                _quizTitle = value;
                OnPropertyChanged(nameof(QuizTitle));
            }
        }      

        private string _questionStatement;
        public string QuestionStatement
        {
            get => _questionStatement;
            set
            {
                _questionStatement = value;
                OnPropertyChanged(nameof(QuestionStatement));
            }
        }
        private List<string> _answers;
        public List<string> Answers
        {
            get => _answers;
            set
            {
                _answers = value;
                OnPropertyChanged(nameof(Answers));
            }
        }
        private int _correctAnswer;
        public int CorrectAnswer
        {
            get => _correctAnswer;
            set
            {
                _correctAnswer = value;
                OnPropertyChanged(nameof(CorrectAnswer));
            }
        }
        private string _category;
        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        private bool _isModified;
        public bool IsModified
        {
            get => _isModified;
            set
            {
                _isModified = value;
                OnPropertyChanged(nameof(IsModified));
            }
        }

        private bool _canSaveQuestion;
        public bool CanSaveQuestion
        {
            get => _canSaveQuestion;
            set
            {
                _canSaveQuestion = value;
                OnPropertyChanged(nameof(CanSaveQuestion));
            }
        }

        private bool _canSaveQuiz;
        public bool CanSaveQuiz
        {
            get => _canSaveQuiz;
            set
            {
                _canSaveQuiz = value;
                OnPropertyChanged(nameof(CanSaveQuiz));
            }
        }

        private bool _hasQuestions;
        public bool HasQuestions
        {
            get => _hasQuestions;
            set
            {
                _hasQuestions = value;
                OnPropertyChanged(nameof(HasQuestions));
            }
        }
        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        private string _tempImagePath;
        public string TempImagePath
        {
            get => _tempImagePath;
            set
            {
                _tempImagePath = value;
                
                OnPropertyChanged(nameof(TempImagePath));
            }
        }

        public ICommand SaveCommand { get; private set; }  
        public ICommand UpdateQuestionCommand { get; private set; }
        public ICommand NextQuestionCommand { get; private set; }
        public ICommand PreviousQuestionCommand { get; private set; }
        public ICommand BrowseImageCommand { get; private set; }
       
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CreateQuizViewModel()
        {
            CurrentQuiz = new Quiz();
            _dbService = new DbService();               
            Answers = new[] { " ", " ", " " }.ToList();   
            
            SaveCommand = new RelayCommand(SaveQuiz);           
            UpdateQuestionCommand = new RelayCommand(UpdateCurrentQuestion);
            NextQuestionCommand = new RelayCommand(NextQuestion);
            PreviousQuestionCommand = new RelayCommand(PreviousQuestion);
            BrowseImageCommand = new RelayCommand(BrowseImage);
            
            ImageTargetFolder = _dbService.GetImageFolderPath();
            
            IsModified = false;
            CanSaveQuiz = false;
            CanSaveQuestion = true;
            HasQuestions = false;
            ImagePath = Path.Combine(ImageTargetFolder, "PlaceholderImage.png");
        }

        private async void SaveQuiz()
        {
            if (string.IsNullOrEmpty(QuizTitle))
            {
                MessageBox.Show("Please enter a title for the quiz.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var quizDAO = new QuizDAO
            {
                Title = QuizTitle,
                Questions = CurrentQuiz.Questions.ToList()
            }; 
            
            List<string> Files = new List<string>();
            Files = _dbService.GetAllQuizTitles();

            foreach (var file in Files)
            {
                
                if (file.Equals(QuizTitle, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Quiz with this title already exists. Edit the existing quiz or choose a different title.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            await _dbService.WriteData(quizDAO, QuizTitle);
            MessageBox.Show("Quiz saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            QuizCreatedCompleted?.Invoke();
            CurrentQuiz = new Quiz();           
            
        }
   
        private void NextQuestion()
        {
           if(HasQuestions)
            {
                if (IsNextQuestionAvailable)
                {
                    CurrentQuestionIndex++;
                    UpdateQuizData();
                }
                else
                {
                    ClearQuestionData();
                    CurrentQuestionIndex++;
                    
                }
               
            }
        }

        private void PreviousQuestion()
        {
            if (IsPreviousQuestionAvailable)
            {
                CurrentQuestionIndex--;
                UpdateQuizData();
                CanSaveQuestion = false;
                HasQuestions = true;
            }
        }

        private void UpdateCurrentQuestion()
        {
            try
            {
                if (Answers.Any(answer => string.IsNullOrEmpty(answer)) || string.IsNullOrEmpty(Category) || string.IsNullOrEmpty(QuestionStatement))

                {
                    MessageBox.Show("Please fill all the fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(QuizTitle))
                {
                    MessageBox.Show("Please enter a title for the quiz.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newQuestion = new Question(QuestionStatement, Answers.ToArray(), CorrectAnswer, Category, ImageFileName);                

                CurrentQuiz.AddQuestion(newQuestion.Statement, newQuestion.CorrectAnswer, newQuestion.Category, newQuestion.ImageFileName, newQuestion.Answers);

                string targetFileName = Path.GetFileName(ImagePath);
                if (!string.IsNullOrEmpty(targetFileName))
                {
                    var combinedPath = Path.Combine(ImageTargetFolder, targetFileName);
                    
                    if (!ImagePath.Equals(combinedPath, StringComparison.OrdinalIgnoreCase))
                    {
                        var targetPath = Path.Combine(ImageTargetFolder, Path.GetFileName(targetFileName));

                        File.Copy(ImagePath, targetPath, overwrite: true);
                    }
                }


            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            MessageBox.Show("Question updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            
            CanSaveQuiz = true;       
           
            ClearQuestionData();
            CurrentQuestionIndex++;
            

        }
        private void ClearQuestionData()
        {
            QuestionStatement = string.Empty;
            Answers = new[] { " ", " ", " " }.ToList();
            CorrectAnswer = 0;
            Category = string.Empty;
            HasQuestions = false;
            CanSaveQuestion = true;
            ImagePath = Path.Combine(ImageTargetFolder, "PlaceholderImage.png");

        }

        private void UpdateQuizData()
        {
            CurrentQuestion = CurrentQuiz.Questions.ElementAt(_currentQuestionIndex);
            QuestionStatement = CurrentQuestion.Statement;
            Answers = CurrentQuestion.Answers.ToList();
            CorrectAnswer = CurrentQuestion.CorrectAnswer;
            Category = CurrentQuestion.Category;      
            ImagePath = Path.Combine(ImageTargetFolder, CurrentQuestion.ImageFileName);
            
        }

        private void BrowseImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                TempImagePath = openFileDialog.FileName;
                ImageFileName = Path.GetFileName(TempImagePath);
                ImagePath = TempImagePath;
            }
        }
    }
}
