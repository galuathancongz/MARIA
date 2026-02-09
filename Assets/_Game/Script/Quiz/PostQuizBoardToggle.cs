using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class PostQuizBoardToggle : PostQuizBoard
    {
        [Header("Description")]
        public List<string> listStr = new List<string>();
        public List<ButtonClickQuiz> listBtnClickQuiz = new List<ButtonClickQuiz>();
        [Header("Button On Toggle Select")]
        public EMode mode;
        [ShowIf(nameof(mode), EMode.Limited)]
        public int limitedNumber;
        public GameObject goButton;

        [ReadOnly] public List<int> listIndexCanPost = new List<int>();

        public virtual void OnClickButton(int index)
        {
            switch(mode )
            {
                case EMode.Single:
                    listIndexCanPost.Clear();
                    listIndexCanPost.Add(index);
                    break;
                case EMode.Multiple:
                    if (listIndexCanPost.Contains(index))
                    {
                        listIndexCanPost.Remove(index);
                    }
                    else
                    {
                        listIndexCanPost.Add(index);
                    }
                    break;
                case EMode.Limited:
                    if (listIndexCanPost.Contains(index))
                    {
                        listIndexCanPost.Remove(index);
                    }
                    else
                    {
                        if (listIndexCanPost.Count < limitedNumber)
                        {
                            listIndexCanPost.Add(index);
                        }
                    }
                    break;
            }
            UpdateVisual();
        }
        private void UpdateVisual()
        {
            for (int i = 0; i < listBtnClickQuiz.Count; i++)
            {
                int indexFor = i;
                ButtonClickQuiz btn = listBtnClickQuiz[indexFor];
                if (btn != null)
                {
                    bool isSelect = listIndexCanPost.Contains(indexFor);
                    btn.Select(isSelect);
                }
            }
            bool isOpenButton = (listIndexCanPost.Count > 0);
            goButton.SetActive(isOpenButton);
        }


        protected override void OnValidate()
        {
            base.OnValidate();
            var allBtnClickQuiz = transform.GetComponentsInChildren<ButtonClickQuiz>();
            if(listBtnClickQuiz == null || listBtnClickQuiz.Count != allBtnClickQuiz.Length)
            {
                listBtnClickQuiz = allBtnClickQuiz.ToList();
            }
            if (listStr == null || listBtnClickQuiz == null) return;
            if (listStr.Count <= 0) return;
            for (int i = 0; i < listStr.Count; i++)
            {
                if (!listBtnClickQuiz[i]) continue;
                if(string.IsNullOrEmpty(listStr[i])) continue;
                if (i < listBtnClickQuiz.Count)
                {
                    listBtnClickQuiz[i].SetText(listStr[i]);
                }
            }

        }
        public enum EMode
        {
            Single = 0,
            Multiple = 1,
            Limited = 2,
        }
    }
}
