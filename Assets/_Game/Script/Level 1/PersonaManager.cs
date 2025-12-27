using Sirenix.OdinInspector;
using System.Collections.Generic;
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
        public void SetPersonalAmount(EPersonaType type, int amount)
        {
            for (int i = 0; i < personaStats.Count; i++)
            {
                if (personaStats[i].type == type)
                {
                    var persona = personaStats[i];
                    persona.amount = amount;
                    personaStats[i] = persona;
                    return;
                }
            }

            // Nếu chưa có thì thêm mới
            personaStats.Add(new PersonaStat()
            {
                type = type,
                amount = amount
            });
        }

    }
    [System.Serializable]
    public struct PersonaStat
    {
        public EPersonaType type;
        public int amount;
    }
    [System.Serializable]
    public enum EPersonaType
    {
        Creative = 0,
        Logic = 1,
        Empathy = 2,
        Structure = 3,
    }
}
