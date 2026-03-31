using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Luzart
{
	public class PersonaPointComponent : MonoBehaviour
	{
		[SerializeField] private StepKey stepKey;
		[SerializeField] private EPersonaType personaType;
		public void OnClick()
		{
			AddPersonaPoint(stepKey, personaType);
        }
        public void AddPersonaPoint(StepKey stepKey, EPersonaType personaType)
		{
			// Mỗi step chỉ tính 1 điểm. Chơi lại thì cập nhật type nhưng không cộng thêm.
			PersonaManager.Instance.SetPersonaByStepKey(stepKey, personaType, 1);
        }

    }
}
