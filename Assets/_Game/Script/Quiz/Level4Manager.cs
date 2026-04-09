using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class Level4Manager : SingletonSaveLoad<Level4Data, Level4Manager>
    {
        protected override string KEYLOAD => "Level_4";

        public void AddQuestion(Level4Question data)
        {
            if(data is Level4QuestionToggle)
            {
                AddQuestionToggle(data as Level4QuestionToggle);
            }
            else if(data is Level4String)
            {
                AddQuestionString(data as Level4String);
            }
        }
        public void AddQuestionToggle(Level4QuestionToggle data)
        {
            var question = Data.listQuestion.FirstOrDefault(q => q.indexQuestion == data.indexQuestion);
            if (question != null)
            {
                Data.listQuestion.Remove(question);
            }
            Data.listQuestion.Add(data);
        }
        public void AddQuestionString(Level4String data)
        {
            var question = Data.listString.FirstOrDefault(q => q.indexQuestion == data.indexQuestion);
            if (question != null)
            {
                Data.listString.Remove(question);
            }
            Data.listString.Add(data);
        }
    }
    [Serializable]
    public class Level4Data
    {
        public List<Level4QuestionToggle> listQuestion = new();
        public List<Level4String> listString = new();
    }
    [Serializable]
    public class Level4Question
    {
        public int indexQuestion;
    }
    [Serializable]
    public class Level4QuestionToggle : Level4Question
    {
        public List<int> indexAnswer = new();
    }
    [Serializable]
    public class Level4String : Level4Question
    {
        public string str;
    }
}
