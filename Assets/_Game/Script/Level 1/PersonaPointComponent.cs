using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Luzart
{
	public class PersonaPointComponent : MonoBehaviour
	{
		public void AddPersonaPoint(StepKey stepKey, EPersonaType personaType)
		{
			var persona = PersonaManager.Instance.GetPersonaByStepKey(stepKey);
			if(persona == null)
			{
				PersonaManager.Instance.SetPersonaByStepKey(stepKey, personaType, 1);
			}
			else
			{
				int currentPoint = persona.Value.amount;
				int nextPoint = currentPoint + 1;
				PersonaManager.Instance.SetPersonaByStepKey(stepKey, personaType, nextPoint);
            }
        }
    }
}
