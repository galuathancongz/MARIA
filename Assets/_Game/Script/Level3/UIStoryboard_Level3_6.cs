using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Luzart
{
    public class UIStoryboard_Level3_6 : Storyboard
    {
        public TMP_InputField inputField;
        public Transform content;
        public Item_FoldOut itemFoldOut;
        public List<Item_FoldOut > listItemFoldOut = new List<Item_FoldOut>();
        public List<ItemClickImport> listItemClickImport = new List<ItemClickImport>();
        List<(string title, string content)> dataTitle = new List<(string title, string content)>();
        private Level3Data Data => Level3Manager.Instance.Data;
        string changedText = "";
        private void OnEnable()
        {
            inputField.onValueChanged.AddListener(OnInputValueChanged);
        }
        private void OnDisable()
        {
            inputField.onValueChanged.RemoveListener(OnInputValueChanged);
        }
        private void OnInputValueChanged(string arg0)
        {
            changedText = arg0;
        }

        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            var dataTitle = Data.GetTitleSummary();
            var dataObjective = Data.GetObjectiveSummary();
            var dataActivities = Data.GetActivitiesSummary();
            var dataAssessment = Data.GetAssessmentSummary();
            this.dataTitle.Add( ("Lesson Title", dataTitle) );
            this.dataTitle.Add( ("Learning Objectives", dataObjective) );
            this.dataTitle.Add( ("Learning Activities", dataActivities) );
            this.dataTitle.Add( ("Assessment Methods", dataAssessment) );
            MasterHelper.InitListObj(4,itemFoldOut, listItemFoldOut, content, (obj, index) =>
            {
                obj.gameObject.SetActive(true);
                var (title, content) = this.dataTitle[index];
                obj.Setup(title, content);
            });
            foreach (var item in listItemClickImport)
            {
                item.Initialize(ClickItem);
            }
        }
        private void ClickItem(string str)
        {
            inputField.text =  str;
        }
        public void OnClickSubmit()
        {
            Data.listFeedbackSelected.Add(changedText);
        }
    }
}
