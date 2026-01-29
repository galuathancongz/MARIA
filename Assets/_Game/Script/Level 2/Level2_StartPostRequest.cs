using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Luzart
{
    public class Level2_StartPostRequest : MonoBehaviour
    {
        public string strRequest = "Hello, AI Mentor! This is start Ideation Lab! Not response this message!";
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
