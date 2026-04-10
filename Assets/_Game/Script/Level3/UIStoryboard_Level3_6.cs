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
        public TMP_Text txtPersona;
        public TMP_Text txtMentorSubject;
        public TMP_Text txtRevisions;
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
            txtPersona.text = PersonaManager.Instance.GetStringPersonaType();
            txtMentorSubject.text = MentorSubjectExtension.GetNameMentor(Level3Manager.Instance.Data.subject);
            txtRevisions.text = Level3Manager.Instance.Data.GetAllSendAI().ToString();
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
            try
            {
                base.Show(onHideDone);
                // Build summary from current context sections
                this.dataTitle.Clear();
                this.dataTitle.Add((Loc.K("level3.lesson_title"), Data.GetTitleSummary()));
                this.dataTitle.Add((Loc.K("level3.learning_objective"), Data.GetObjectiveSummary()));

                // Add each lesson section from current context
                var sections = Data.GetCurrentSections();
                foreach (var s in sections)
                {
                    string name = LessonPlanTemplate.GetSectionName(s.index);
                    this.dataTitle.Add((name, s.content));
                }

                this.dataTitle.Add((Loc.K("level3.feedback_summary"), Data.GetAssessmentSummary()));
                MasterHelper.InitListObj(this.dataTitle.Count, itemFoldOut, listItemFoldOut, content, (obj, index) =>
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
            catch (Exception ex)
            {
                Debug.LogError($"Error in Show method: {ex.Message}");
                inputField.text = $"Error loading data. Please try again. {ex}";
            }

        }
        private void ClickItem(string str)
        {
            inputField.text =  str;
        }
        public void OnClickSubmit()
        {
            Data.listFeedbackSelected.Add(changedText);
            SyncManager.Instance.ForceSyncNow(trigger: "level3.submit");
        }
    }
}
