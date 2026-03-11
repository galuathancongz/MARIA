using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Luzart
{
    public class UITop : MonoBehaviour
    {
        public Button btnBack;
        public Button btnNext;
        private void Awake()
        {
            ShowBtnBack(false);
            ShowBtnNext(false);
        }
        public void ShowBtnBack(bool isShow)
        {
            btnBack.gameObject.SetActive(isShow);
        }
        public void ShowBtnNext(bool isShow)
        {
            btnNext.gameObject.SetActive(isShow);
        }
        public void OnClickBtnBack()
        {
            UIManager.Instance.ShowBackScenario();
        }
        public void OnClickBtnNext()
        {
            UIManager.Instance.ShowNextScenario();
        }
        public void OnClickSettings()
        {
            UIManager.Instance.ShowUI(UIName.Settings);
        }
        public void OnClickProfile()
        {
            UIManager.Instance.ShowUI(UIName.Profile);
        }
    }
}
