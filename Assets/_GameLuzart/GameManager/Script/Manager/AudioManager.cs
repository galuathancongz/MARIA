namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class AudioManager : Singleton<AudioManager>
    {
        private AudioSource audioSFX;
        private AudioSource audioMusic;
        private const float DEFAULT_VOLUME = 1f; // Giá trị mặc định khi không mute
    
        private const string VOLUMN_SFX = "volumn_sfx";
        private const string VOLUMN_MUSIC = "volumn_music";
        private const string MUTE_VIBRA = "mute_vibra";
    
        public float volumnSFX
        {
            get => PlayerPrefs.GetFloat(VOLUMN_SFX, DEFAULT_VOLUME);
            set
            {
                audioSFX.volume = value;
                PlayerPrefs.SetFloat(VOLUMN_SFX, value);
                PlayerPrefs.Save();
            }
        }
    
        public bool isMuteSFX
        {
            get => volumnSFX == 0;
            set => volumnSFX = value ? 0 : DEFAULT_VOLUME;
        }
    
        public float volumnMusic
        {
            get => PlayerPrefs.GetFloat(VOLUMN_MUSIC, DEFAULT_VOLUME);
            set
            {
                audioMusic.volume = value;
                PlayerPrefs.SetFloat(VOLUMN_MUSIC, value);
                PlayerPrefs.Save();
            }
        }
    
        public bool isMuteMusic
        {
            get => volumnMusic == 0;
            set => volumnMusic = value ? 0 : DEFAULT_VOLUME;
        }
    
        public bool isMuteVibra
        {
            get => PlayerPrefs.GetInt(MUTE_VIBRA, 0) == 0;
            set
            {
                PlayerPrefs.SetInt(MUTE_VIBRA, value ? 0 : 1); // 0: mute, 1: bật
                PlayerPrefs.Save();
            }
        }
    
        [SerializeField] private AudioClip audioClick;
    
    
        private void Awake()
        {
            audioSFX = gameObject.AddComponent<AudioSource>();
            audioMusic = gameObject.AddComponent<AudioSource>();
            audioSFX.mute = isMuteSFX;
            audioMusic.mute = isMuteMusic;
        }
        public void PlaySFXBtn()
        {
            audioSFX.PlayOneShot(audioClick);
        }
        public void PlaySFXCoin()
        {
            //audioSFX.PlayOneShot(audioClick);
        }
        public void Vibrate()
        {
            if (isMuteVibra)
            {
                return;
            }
        }
        private void OnApplicationQuit()
        {
            PlayerPrefs.Save();
        }
    }
}
