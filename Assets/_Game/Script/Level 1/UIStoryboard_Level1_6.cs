using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class UIStoryboard_Level1_6 : Storyboard
    {
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            Level2Manager.IsSendStartIdeationLab = false;

            // Badge: completed Scene 5 reflection journal
            SkillManager.Instance?.UnlockSkill(ESkillId.ReflectionJournal);
        }
        public void OnClickMainMenu()
        {
            UIManager.Instance.HideAll();
            UIManager.Instance.ShowUI(UIName.MainMenu);
        }
        public void OnClickNextScenario()
        {
            UIManager.Instance.ShowNextScenario();
        }
    }
}
