namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class SoundSettings : MonoBehaviour
    {
        private bool isSetup { get; set; } = false;
    
        protected virtual void Setup()
        {
        }
        public virtual void Show()
        {
            if(!isSetup)
            {
                Setup();
                isSetup = true;
            }
        }
        
    }
}
