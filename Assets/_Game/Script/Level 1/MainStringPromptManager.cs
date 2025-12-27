using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Luzart
{
	public class MainStringPromptManager : SingletonSaveLoad<StringPromptData, MainStringPromptManager>
	{
        protected override string KEYLOAD => "MainStringPrompt";
	}

	[System.Serializable]
    public class StringPromptData
	{
		public List<StringPromptPair> stringPromptPairs = new List<StringPromptPair>();
		public void SetStringPrompt(StepKey stepKey, string prompt)
		{
			for (int i = 0; i < stringPromptPairs.Count; i++)
			{
				if (stringPromptPairs[i].stepKey == stepKey)
				{
					stringPromptPairs[i].prompt = prompt;
					return;
				}
            }
			StringPromptPair newPair = new StringPromptPair();
			newPair.stepKey = stepKey;
			newPair.prompt = prompt;
			stringPromptPairs.Add(newPair);
        }
		public string GetStringPrompt(StepKey stepKey)
		{
			for (int i = 0; i < stringPromptPairs.Count; i++)
			{
				if (stringPromptPairs[i].stepKey == stepKey)
				{
					return stringPromptPairs[i].prompt;
				}
			}
			return string.Empty;
        }
    }
	[System.Serializable]
	public class StringPromptPair
	{
		public StepKey stepKey;
		public string prompt;
    }

    public enum StepKey
    {
        None = 0,
        L1_S1_1 = 1,
		L1_S1_2 = 2,
		L1_S1_3 = 3,
		L1_S1_4 = 4,
    }
}
