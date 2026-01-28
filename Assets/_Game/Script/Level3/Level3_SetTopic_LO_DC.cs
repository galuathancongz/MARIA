using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
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
        [ContextMenu("Auto Set Text")]
        private void OnAutoSetText()
        {
            var allTxt = transform.parent.GetComponentsInChildren<TMP_Text>();
            foreach (var txt in allTxt)
            {
                if (txt.name.Equals("txtTitle"))
                {
                    topic = txt.text;
                }
                else if (txt.name.Contains("LearningObject"))
                {
                    learningObjective = txt.text;
                }
                else if (txt.name.Contains("Design"))
                {
                    designContraints = txt.text;
                }
            }
        }
    }
}
