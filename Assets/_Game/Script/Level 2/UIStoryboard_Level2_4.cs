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
            return "Context: Bạn là trợ lý ảo MARIA. Nhiệm vụ của bạn là tổng kết hành trình của giáo viên trong Ideation Lab.\n" +
           "Task: Phân tích hành trình và trả về một đối tượng JSON duy nhất để hiển thị màn hình tổng kết[cite: 52, 53].\n" +
           "Yêu cầu kỹ thuật:\n" +
           "1. Phản hồi CHỈ chứa mã JSON, không có dấu nháy ngược (markdown), không có lời dẫn.\n" +
           "2. Các trường 'explored_methods', 'ai_capabilities', 'most_used_tools' PHẢI là mảng các chuỗi (Array of strings).\n" +
           "3. 'curiosity_score' là một số nguyên từ 1 đến 5 dựa trên mức độ tương tác.\n" +
           "Output JSON Format:\n" +
           "{\n" +
           "  \"curiosity_score\": 4,\n" +
           "  \"explored_methods\": [\"Phương pháp 1\", \"Phương pháp 2\"],\n" +
           "  \"ai_capabilities\": [\"Khả năng 1\", \"Khả năng 2\"],\n" +
           "  \"most_used_tools\": [\"Công cụ 1\", \"Công cụ 2\"]\n" +
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
