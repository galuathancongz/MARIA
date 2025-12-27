using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Luzart
{
	public class PersonaPointComponent : MonoBehaviour
	{
		public void SetPersonaData(EPersonaType type, int amount)
		{
			PersonaManager.Instance.Data.SetPersonalAmount(type, amount);
		}
		public void AddPersonaData(EPersonaType type, int amount)
		{
			var currentAmount = PersonaManager.Instance.Data.GetPersonaAmount(type);
			int newAmount = currentAmount + amount;
			PersonaManager.Instance.Data.SetPersonalAmount(type, newAmount);
		}
		public void RemovePersonaData(EPersonaType type, int amount)
		{
            var currentAmount = PersonaManager.Instance.Data.GetPersonaAmount(type);
            int newAmount = currentAmount - amount;
            PersonaManager.Instance.Data.SetPersonalAmount(type, newAmount);
        }
	}
}
