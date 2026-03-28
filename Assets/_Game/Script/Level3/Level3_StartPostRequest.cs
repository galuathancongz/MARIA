using UnityEngine;

namespace Luzart
{
    public class Level3_StartPostRequest : MonoBehaviour
    {
        public string strRequest => LocalizationManager.Instance.Get("prompts.level3_start");
        private void Start()
        {
            SendPostRequest();
        }
        void SendPostRequest()
        {
            if (!Level3Manager.IsSendStartCoCreatorStudio)
            {
                APIManager.Instance.Send(strRequest, null);
                Level3Manager.IsSendStartCoCreatorStudio = true;
            }
        }
    }
}
