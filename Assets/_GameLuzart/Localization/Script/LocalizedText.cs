namespace Luzart
{
    using TMPro;
    using UnityEngine;

    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string locKey;
        private TMP_Text textComponent;

        private void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            UpdateText();
            Observer.Instance?.AddObserver(ObserverKey.OnLanguageChanged, OnLanguageChanged);
        }

        private void OnDestroy()
        {
            Observer.Instance?.RemoveObserver(ObserverKey.OnLanguageChanged, OnLanguageChanged);
        }

        private void OnLanguageChanged(object data)
        {
            UpdateText();
        }

        public void UpdateText()
        {
            if (textComponent != null && !string.IsNullOrEmpty(locKey) && LocalizationManager.Instance != null)
            {
                textComponent.text = LocalizationManager.Instance.Get(locKey);
            }
        }

        public void SetKey(string key)
        {
            locKey = key;
            UpdateText();
        }
    }
}
