using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Luzart
{
    public class StepAction_Level1Scene2Logic : StepAction
    {
        [SerializeField] private StepActionSwitch stepActionSwitch;
        public override void Execute(Action<ActionResult> _onComplete)
        {
            base.Execute(_onComplete);
            if (PersonaManager.Instance.Data.IsUnlockedAllPersona)
            {
                // Badge: customised all avatar/persona elements
                SkillManager.Instance?.UnlockSkill(ESkillId.PersonalTouch);
                stepActionSwitch.UseStep(0);
            }
            else
            {
                stepActionSwitch.UseStep(1);
            }
            CallOnComplete();
        }
    }

}
