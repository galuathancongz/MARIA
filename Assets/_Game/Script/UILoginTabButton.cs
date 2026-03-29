namespace Luzart
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using Luzart.NewBase;

    /// <summary>
    /// Tab button for the UILogin screen.
    /// Extends BaseSelect directly — we only need Select(bool) for visual toggle.
    /// SelectToggle would add _isSelect caching and SelectInvert() which are unused here.
    /// </summary>
    public class UILoginTabButton : BaseSelect
    {
        [Header("Visuals")]
        public Image    tabBackground;
        public TMP_Text tabLabel;

        [Header("Colors")]
        public Color colorSelected       = new Color(0f,   0.80f, 1f,    1f);
        public Color colorDeselected     = new Color(0.1f, 0.1f,  0.15f, 0.9f);
        public Color textColorSelected   = Color.black;
        public Color textColorDeselected = new Color(0.7f, 0.9f,  1f,   1f);

        /// <summary>
        /// Called by UILogin.SwitchTab() to update visuals only.
        /// Never call OnTabSelected from here — would cause infinite loop:
        /// SwitchTab → Select → event → SwitchTab → ...
        /// </summary>
        public override void Select(bool value)
        {
            if (tabBackground != null)
                tabBackground.color = value ? colorSelected : colorDeselected;

            if (tabLabel != null)
                tabLabel.color = value ? textColorSelected : textColorDeselected;
        }
    }
}
