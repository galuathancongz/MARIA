using UnityEngine;

namespace Luzart
{
    public class Level3_StartPostRequest : MonoBehaviour
    {
        public string strRequest = "Hello, AI Mentor! This is start Co-design Studio! Not response this message!";
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
