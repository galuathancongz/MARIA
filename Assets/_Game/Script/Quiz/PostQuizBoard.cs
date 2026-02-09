using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Luzart
{
    public class PostQuizBoard : MonoBehaviour
    {
        [Header("Post Quiz Board")]
        public BaseSelect bsSections;
        public int index;
        [Header("Post Sections")]
        public string strSections;
        public TMP_Text txtSections;
        [Header("Title")]
        public string strQuestion;
        public TMP_Text txtQuestion;
        [Header("Description")]
        public List<string> listStr = new List<string>();
        public List<TMP_Text> listTxt = new List<TMP_Text>();
        [Header("Button On Toggle Select")]
        public BaseSelect bsToggles;
        public GameObject goButton;

        [ReadOnly] public List<int> listIndexCanPost = new List<int>();

        public virtual void OnClickButton(int index)
        {
            if (bsToggles != null)
            {
                bsToggles.Select(index);
                listIndexCanPost.Clear();
                if(!listIndexCanPost.Contains(index))
                {
                    listIndexCanPost.Add(index);
                }
                
            }
            else
            {
                if(listIndexCanPost.Contains(index))
                {
                    listIndexCanPost.Remove(index);
                }
                else
                {
                    listIndexCanPost.Add(index);
                }
            }
        }
        public virtual void OnClickNextButton()
        {

        }

        private void OnValidate()
        {
            if(txtSections != null)
            {
                txtSections.text = strSections;
            }
            if(txtQuestion != null)
            {
                txtQuestion.text = strQuestion;
            }
            if (listStr == null || listTxt == null) return;
            if (listStr.Count <= 0) return;
            for (int i = 0; i < listStr.Count; i++)
            {
                if (!listTxt[i]) continue;
                if(string.IsNullOrEmpty(listStr[i])) continue;
                if (i < listTxt.Count)
                {
                    listTxt[i].text = listStr[i];
                }
            }
            if(bsSections != null)
            {
                bsSections.Select(index);
            }
        }
    }
}
