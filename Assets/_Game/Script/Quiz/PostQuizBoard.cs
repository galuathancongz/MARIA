using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Luzart
{
    public class PostQuizBoard : MonoBehaviour
    {
        [Header("Level Index")]
        public int levelIndex;
        [Header("Post Quiz Board")]
        public BaseSelect bsSections;
        public int index;
        [Header("Post Sections")]
        public string strSections;
        public TMP_Text txtSections;
        [Header("Title")]
        public string strQuestion;
        public TMP_Text txtQuestion;
        public UnityEvent onClickNextButton;
        public virtual void OnClickNextButton()
        {
            onClickNextButton?.Invoke();
        }
        private void Start()
        {
            LocalizeTexts();
        }
        public void LocalizeTexts()
        {
            if (txtSections != null)
                txtSections.text = Loc.T(strSections);
            if (txtQuestion != null)
                txtQuestion.text = Loc.T(strQuestion);
        }
        protected virtual void OnValidate()
        {
            if (txtSections != null)
            {
                txtSections.text = strSections;
            }
            if (txtQuestion != null)
            {
                txtQuestion.text = strQuestion;
            }
            if (bsSections != null)
            {
                bsSections.Select(index);
            }
        }
    }
}
