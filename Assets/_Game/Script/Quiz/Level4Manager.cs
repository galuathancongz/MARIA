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
            int length = Data.listQuestion.Count;
            for (int i = 0; i < length; i++)
            {
                if(Data.listQuestion[i].indexQuestion == data.indexQuestion)
                {
                    Data.listQuestion.RemoveAt(i);
                    break;
                }
            }
            Data.listQuestion.Add(data);

        }
    }
    [Serializable]
    public class Level4Data
    {
        public List<Level4Question> listQuestion = new();
    }
    [Serializable]
    public class Level4Question
    {
        public int indexQuestion;
        public List<int> indexAnswer = new();
    }
}
