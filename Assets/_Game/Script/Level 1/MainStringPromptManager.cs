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
	[System.Serializable]
    public enum StepKey
    {
        None = 0,
        Level1_Scene1_1 = 1,
		Level1_Scene1_2 = 2,
		Level1_Scene1_3 = 3,
		Level1_Scene1_4 = 4,
		Level1_Scene2_1 = 5,
		Level1_Scene2_2 = 6,
		Level1_Scene2_3 = 7,
		Level1_Scene2_4 = 8,
		Level1_Scene3_1 = 9,
		Level1_Scene3_2 = 10,
		Level1_Scene3_3 = 11,
		Level1_Scene3_4 = 12,
        Level1_Scene3_5 = 13,
        Level1_Scene3_6 = 14,
    }
}
