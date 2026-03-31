using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class Level2_Scene1_SelectLogicStepAction : StepAction
    {
        public BaseSelect bs;
        public StepActionSwitch stepActionSwitch;
        public override void Execute(Action<ActionResult> _onComplete)
        {
            base.Execute(_onComplete);
            int index = (int)Level2Manager.Instance.Data.subject;
            bs.Select(index);
            stepActionSwitch.UseStep(index);
            var question = LocalizationManager.Instance.GetPrompt("prompts.level2_1_context", new System.Collections.Generic.Dictionary<string, string> {
                {"mentorName", Level2Manager.Instance.Data.MentorName},
                {"subjectName", Level2Manager.Instance.Data.SubjectName}
            });
            APIManager.Instance.Send(question, null);
            CallOnComplete();
        }
    }
}
