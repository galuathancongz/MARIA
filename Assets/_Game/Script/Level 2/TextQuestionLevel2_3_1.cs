using TMPro;
using UnityEngine;

namespace Luzart
{
    public class TextQuestionLevel2_3_1 : MonoBehaviour
    {
        public TMP_Text txtQuestion;

        private void OnEnable()
        {
            txtQuestion.text = Level2Manager.Instance.Data.question2_3_1;
        }
    }
}
