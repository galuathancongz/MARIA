using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Luzart
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Tooltip")]
        public string tooltipKey;
        public GameObject tooltipPanel;
        public TMP_Text tooltipText;
        public TweenAnimationBase showAnimation;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltipPanel == null) return;

            if (tooltipText != null && !string.IsNullOrEmpty(tooltipKey))
            {
                tooltipText.text = LocalizationManager.Instance.Get(tooltipKey);
            }

            tooltipPanel.SetActive(true);
            showAnimation?.Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Hide();
        }

        private void Hide()
        {
            showAnimation?.Stop();
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        private void OnDisable()
        {
            Hide();
        }
    }
}
