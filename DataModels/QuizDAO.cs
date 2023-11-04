using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3.DataModels
{
    public class QuizDAO
    {       
            public string Title { get; set; }
            public List<Question> Questions { get; set; }

        public QuizDAO() 
        {
            Questions = new List<Question>();
            Title = string.Empty;
        }
    }
}
