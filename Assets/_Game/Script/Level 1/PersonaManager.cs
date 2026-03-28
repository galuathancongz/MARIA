using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Luzart
{
    public class PersonaManager : SingletonSaveLoad<PersonaData, PersonaManager>
    {
        protected override string KEYLOAD => "Persona";
        public string GetStringPersonaType()
        {
            EPersonaType type = GetMyPersonaType();
            switch (type)
            {
                case EPersonaType.Creative:
                    return LocalizationManager.Instance.Get("persona.creative");
                case EPersonaType.LogicOrStruct:
                    return LocalizationManager.Instance.Get("persona.logical");
                case EPersonaType.Empathy:
                    return LocalizationManager.Instance.Get("persona.empathetic");
                default:
                    return LocalizationManager.Instance.Get("persona.unknown");
            }
        }

        public string GetHexColorPersonaType()
        {
            EPersonaType type = GetMyPersonaType();
            switch (type)
            {
                case EPersonaType.Creative:
                    return "#FFA500"; // Orange
                case EPersonaType.LogicOrStruct:
                    return "#0000FF"; // Blue
                case EPersonaType.Empathy:
                    return "#008000"; // Green
                default:
                    return "#000000"; // Black
            }
        }
        public string GetNameColorPersonaType()
        {
            EPersonaType type = GetMyPersonaType();
            switch (type)
            {
                case EPersonaType.Creative:
                    return LocalizationManager.Instance.Get("persona.color.orange");
                case EPersonaType.LogicOrStruct:
                    return LocalizationManager.Instance.Get("persona.color.blue");
                case EPersonaType.Empathy:
                    return LocalizationManager.Instance.Get("persona.color.green");
                default:
                    return LocalizationManager.Instance.Get("persona.color.black");
            }
        }
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
            var dict = GetDictPersona();
            if (dict.ContainsKey(type))
            {
                return dict[type];
            }
            else
            {
                return 0;
            }
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
                var dict = GetDictPersona();
                if(dict.Count == 0)
                {
                    return 0;
                }
                int maxPointDict = dict.Max(x=>x.Value);
                return maxPointDict;
            }
        }
        public bool IsUnlockedAllPersona
        {
            get
            {
                var dict = GetDictPersona();
                var allValue = dict.Select(x => x.Value).ToList();
                var countValueInEnum = Enum.GetValues(typeof(EPersonaType)).Length;
                if (allValue.Count < countValueInEnum)
                {
                    return false;
                }
                foreach(var item in allValue)
                {
                    if(item < 1)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        private Dictionary<EPersonaType, int> GetDictPersona()
        {
            Dictionary<EPersonaType, int> dict = new Dictionary<EPersonaType, int>();
            int length = personaStats.Count;
            for (int i = 0; i < length; i++)
            {
                var persona = personaStats[i];
                if (!dict.ContainsKey(persona.type))
                {
                    dict[persona.type] = 0;
                }
                dict[persona.type] += persona.amount;
            }
            return dict;
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
        LogicOrStruct = 1,
        Empathy = 2,
    }
}
