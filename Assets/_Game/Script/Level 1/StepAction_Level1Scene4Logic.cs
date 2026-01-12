using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class StepAction_Level1Scene4Logic : StepAction
    {
        [SerializeField]
        private BaseSelect baseSelect;
        public override void Execute(Action<ActionResult> _onComplete)
        {
            base.Execute(_onComplete);
            int indexPersona = (int)PersonaManager.Instance.GetMyPersonaType();
            baseSelect.Select(indexPersona);
            onComplete?.Invoke(new ActionResult(actionResultType));
        }
    }
}
