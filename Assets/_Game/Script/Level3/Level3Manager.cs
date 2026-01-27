using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class Level3Manager : SingletonSaveLoad<Level3Data, Level3Manager>
    {
        protected override string KEYLOAD => "Level3_Data";

    }
    
    [Serializable]
    public class Level3Data
    {
        public string topic;
        public string learningObjective;
        public string designContraints;
        public List<string> optionalFilters;
    }
}
