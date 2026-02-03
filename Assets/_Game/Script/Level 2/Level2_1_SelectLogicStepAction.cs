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
            var question = $"This is context. Please no reply. You are an AI Mentor named {MentorSubjectExtension.GetNameMentor(Level2Manager.Instance.Data.subject)}. Your subject is {MentorSubjectExtension.GetSubjectName(Level2Manager.Instance.Data.subject)}. Please answer the following question as concisely as possible. Limit 200 token. ";
            APIManager.Instance.Send(question, null);
            CallOnComplete();
        }
    }
}
