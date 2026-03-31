using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luzart
{
    public class UIBadgeUnlock : UIBase
    {
        [Header("Badge Card")]
        public GameObject cardPanel;
        public Image      imgIcon;
        public TMP_Text   txtTitle;
        public TMP_Text   txtDesc;

        private readonly Queue<ESkillId> _queue = new Queue<ESkillId>();
        private bool _showing = false; // card đang hiện hay không

        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
        }

        public void Enqueue(ESkillId id)
        {
            _queue.Enqueue(id);

            if (!_showing)
                ShowNext();
        }

        public override void OnClickClose()
        {
            if (_queue.Count > 0)
            {
                ShowNext();
            }
            else
            {
                _showing = false;
                if (cardPanel != null) cardPanel.SetActive(false);
                base.OnClickClose();
            }
        }

        private void ShowNext()
        {
            if (_queue.Count == 0) return;

            var id   = _queue.Dequeue();
            var info = SkillDefinition.Get(id);
            var cfg  = SkillConfigDatabase.Instance?.Get(id);
            if (info == null) return;

            if (imgIcon != null && cfg?.icon != null)
                imgIcon.sprite = cfg.icon;

            if (txtTitle != null)
                txtTitle.text = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.Get(info.nameKey)
                    : info.nameKey;

            if (txtDesc != null)
                txtDesc.text = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.Get(info.descKey)
                    : info.descKey;

            if (cardPanel != null) cardPanel.SetActive(true);
            _showing = true;
        }
    }
}
