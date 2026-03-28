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
            return LocalizationManager.Instance.Get("prompts.level2_4_summary");
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
