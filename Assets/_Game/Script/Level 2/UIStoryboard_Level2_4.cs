using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Luzart
{
    public class UIStoryboard_Level2_4 : Storyboard
    {
        public ListDataResponeItem youExplored;
        public ListDataResponeItem aiHelped;
        public ListDataResponeItem mostUsedTools;
        public ProgressBarUI progressBarUI;
        public BaseSelect bsStar;
        public TMP_Text txtPercent;
        private Level2_4_Data _data = new Level2_4_Data();
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            UIManager.Instance.ShowLoading();
            SendRequestGetData();
        }
        public void SendRequestGetData()
        {
            Level2Manager.Instance.Send(2, GetRequest(), OnResultString);
        }
        private string GetRequest()
        {
            return "Context: You are the virtual assistant MARIA. Your mission is to summarize the teacher's journey in the Ideation Lab.\n" +
                   "Task: Analyze the journey and return a single JSON object for the summary screen display.\n" +
                   "- curiosity_score: An integer (1-5) representing the user's curiosity and engagement with AI.\n" +
                   "- explored_methods: Array of methods explored, such as: Role-play, inquiry-based learning, teachback with AI approach.\n" +
                   "- ai_capabilities: Array of AI capabilities used, such as: Generate simulations, character scripts, guiding student learning, and creating group activities.\n" +
                   "- most_used_tools: Array of tools used, such as: Prompt generation, dialogue builder, classroom scenario editor.\n\n" +
                   "Technical Requirements:\n" +
                   "1. Response must contain ONLY valid JSON code. No markdown, no backticks, no introductory text.\n" +
                   "2. Fields 'explored_methods', 'ai_capabilities', and 'most_used_tools' MUST be an Array of Strings.\n" +
                   "3. 'curiosity_score' must be an integer between 1 and 5 based on interaction depth.\n\n" +
                   "Output JSON Format:\n" +
                   "{\n" +
                   "  \"curiosity_score\": curiosity_score,\n" +
                   "  \"explored_methods\": [\"Method 1\", \"Method 2\"],\n" +
                   "  \"ai_capabilities\": [\"Capability 1\", \"Capability 2\"],\n" +
                   "  \"most_used_tools\": [\"Tool 1\", \"Tool 2\"]\n" +
                   "}";
        }
        private void OnResultString(string str)
        {
            UIManager.Instance.HideLoading();
            Level2_4_Data json = new Level2_4_Data();
            try
            {
                json = JsonUtility.FromJson<Level2_4_Data>(str);
                _data = json;
            }
            catch (Exception e)
            {
                Debug.LogError($"JSON Parsing Error: {e.Message}");
            }
            DisplayData();
        }
        private void DisplayData()
        {
            youExplored.SetupData(_data.explored_methods.ToList());
            aiHelped.SetupData(_data.ai_capabilities.ToList());
            mostUsedTools.SetupData(_data.most_used_tools.ToList());
            bsStar.Select(_data.curiosity_score);
            float percent = (_data.curiosity_score / 5f) * 100f;
            float sliderValue = percent / 100f;
            progressBarUI.SetSlider(sliderValue, sliderValue,0);
            txtPercent.text = $"{Mathf.RoundToInt(percent)}%";
        }
    }
    [Serializable]
    public class Level2_4_Data
    {
        public int curiosity_score;
        public string[] explored_methods;
        public string[] ai_capabilities;
        public string[] most_used_tools;
    }
}
