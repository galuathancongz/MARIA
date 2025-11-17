namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class SoundSettingsSlider : SoundSettings
    {
        public Slider sliderMusic;
        public TMP_Text txtValueMusic;
        public Slider sliderSFX;
        public TMP_Text txtValueSFX;
    
        protected override void Setup()
        {
            base.Setup();
            sliderMusic.onValueChanged.AddListener(UpdateValueMusic);
            sliderSFX.onValueChanged.AddListener(UpdateValueSFX);
        }
        public override void Show()
        {
            base.Show();
            sliderMusic.value = AudioManager.Instance.volumnMusic * 100f;
            sliderSFX.value = AudioManager.Instance.volumnSFX * 100f;
        }
        private void UpdateValueMusic(float x)
        {
            UpdateText(txtValueMusic, x);
            AudioManager.Instance.volumnMusic = x / 100f;
        }
        private void UpdateValueSFX(float x)
        {
            UpdateText(txtValueSFX, x);
            AudioManager.Instance.volumnSFX = x / 100f;
        }
        private void UpdateText(TMP_Text tx, float x)
        {
            tx.text = $"{x}%";
        }
    }
}
