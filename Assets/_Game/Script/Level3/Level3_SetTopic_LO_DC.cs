using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class Level3_SetTopic_LO_DC : MonoBehaviour
    {
        [SerializeField] private string topic;
        [SerializeField] private string learningObjective;
        [SerializeField] private string designContraints;
        public void OnClick()
        {
            Level3Manager.Instance.Data.topic = topic;
            Level3Manager.Instance.Data.learningObjective = learningObjective;
            Level3Manager.Instance.Data.designContraints = designContraints;
            Level3Manager.Instance.Save();
        }
    }
}
