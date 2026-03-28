using UnityEngine;

namespace Luzart
{
    public class Level2_StartPostRequest : MonoBehaviour
    {
        public string strRequest => LocalizationManager.Instance.Get("prompts.level2_start");
        private void Start()
        {
            SendPostRequest();
        }
        void SendPostRequest()
        {
            if(!Level2Manager.IsSendStartIdeationLab)
            {
                APIManager.Instance.Send(strRequest, null);
                Level2Manager.IsSendStartIdeationLab = true;
            }
        }
    }
}
