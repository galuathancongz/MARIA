using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Luzart
{
    public class PersonaManager : SingletonSaveLoad<PersonaData, PersonaManager>
    {
        protected override string KEYLOAD => "Persona";

        public EPersonaType GetMyPersonaType()
        {
            EPersonaType currentType = EPersonaType.Creative;
            int first = Data.GetPersonaAmount(currentType);
            for (int i = 0; i < Data.PersonaStats.Count; i++)
            {
                var stat = Data.PersonaStats[i];
                if(stat.amount > first)
                {
                    first = stat.amount;
                    currentType = stat.type;
                }
            }
            return currentType;
        }

        public PersonaStat? GetPersonaByStepKey(StepKey stepKey)
        {
            for (int i = 0; i < Data.PersonaStats.Count; i++)
            {
                var data = Data.PersonaStats[i];
                if (data.stepKey == stepKey)
                {
                    return data;
                }
            }
            return null;
        }
        public void SetPersonaByStepKey(StepKey stepKey, EPersonaType type, int amount)
        {
            Data.SetPersonalAmount(stepKey, type, amount);
        }

    }

    [System.Serializable]
    public class PersonaData
    {
        [SerializeField]
        [ReadOnly]
        private List<PersonaStat> personaStats = new List<PersonaStat>();
        public IReadOnlyList<PersonaStat> PersonaStats => personaStats;
        public int GetPersonaAmount(EPersonaType type)
        {
            int length = personaStats.Count;
            for (int i = 0; i < length; i++)
            {
                if (personaStats[i].type == type)
                {
                    return personaStats[i].amount;
                }
            }
            return 0;
        }
        public void SetPersonalAmount(StepKey stepKey, EPersonaType type, int amount)
        {
            for (int i = 0; i < personaStats.Count; i++)
            {
                if (personaStats[i].stepKey == stepKey)
                {
                    var persona = personaStats[i];
                    persona.amount = amount;
                    persona.type = type;
                    personaStats[i] = persona;
                    return;
                }
            }
            personaStats.Add(new PersonaStat()
            {
                stepKey = stepKey,
                type = type,
                amount = amount
            });
            Observer.Instance.Notify(ObserverKey.PersonaDataChange);
        }
        public int MaxPersonaPoint
        {
            get
            {
                int point = 1;
                for (int i = 0; i < personaStats.Count; i++)
                {
                    if (point < personaStats[i].amount)
                    {
                        point = personaStats[i].amount;
                    }
                }
                return point;
            }
        }
        public bool IsUnlockedAllPersona
        {
            get
            {
                int lengthPersona = personaStats.Count;
                int personaCount = Enum.GetValues(typeof(EPersonaType)).Length;
                if(lengthPersona < personaCount)
                {
                    return false;
                }
                for (int i = 0; i < lengthPersona; i++)
                {
                    var persona = personaStats[i];
                    if(persona.amount <= 0)
                    {
                        return false;
                    }
                }
                return true;
                
            }
        }

    }
    [System.Serializable]
    public struct PersonaStat
    {
        public EPersonaType type;
        public int amount;
        public StepKey stepKey;
    }
    [System.Serializable]
    public enum EPersonaType
    {
        Creative = 0,
        Logic = 1,
        Empathy = 2,
    }
}
