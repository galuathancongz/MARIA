namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class SoundSettingsButtonOnOffSelectChange : SoundSettings
    {
        public Button btnSFX, btnMusic, btnHapstic;
    
        public BaseSelect bsSFX, bsMusic, bsHapstic;
    
        protected override void Setup()
        {
            base.Setup();
            SetUpButton();
        }
        public override void Show()
        {
            base.Show();
            //bsMusic.Select(DataGame.Music == 1);
            //bsSFX.Select(DataGame.Sound == 1);
            //bsHapstic.Select(DataGame.Haptics == 1);
        }
        private void SetUpButton()
        {
            GameUtil.ButtonOnClick(btnMusic, ClickMusic, false);
            GameUtil.ButtonOnClick(btnSFX, ClickSFX, false);
            GameUtil.ButtonOnClick(btnHapstic, ClickVibrate, false);
        }
        private void ClickMusic()
        {
            //bool isOn = DataGame.Music == 1;
            //isOn = !isOn;
            //DataGame.Music = isOn ? 1: 0;
            //bsMusic.Select(isOn);
            //if (isOn)
            //{
            //    GameEventManager.RaisedEvent(GameEventManager.EventId.GameAcMusic);
            //}
        }
        private void ClickSFX()
        {
            //bool isOn = DataGame.Sound == 1;
            //isOn = !isOn;
            //DataGame.Sound = isOn ? 1 : 0;
            //bsSFX.Select(isOn);
        }
        private void ClickVibrate()
        {
            //bool isOn = DataGame.Haptics == 1;
            //isOn = !isOn;
            //DataGame.Haptics = isOn ? 1 : 0;
            //bsHapstic.Select(isOn);
        }
    }
}
