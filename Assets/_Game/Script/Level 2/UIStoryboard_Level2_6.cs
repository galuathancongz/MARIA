using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class UIStoryboard_Level2_6 : Storyboard
    {
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);

            // Badges: all 4 AI tools unlock when Level 2 is completed
            SkillManager.Instance?.UnlockSkillsForLevel(2);
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
