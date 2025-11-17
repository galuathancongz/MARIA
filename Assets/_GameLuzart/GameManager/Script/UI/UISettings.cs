namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UISettings : UIBase
    {
        public SoundSettings soundSettings;
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            soundSettings.Show();
        }
    }
}
