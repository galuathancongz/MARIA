using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luzart
{
    // ══════════════════════════════════════════════════════════════════════════
    //  UIBadgeUnlock : UIBase
    //
    //  Popup khi unlock badge. Show lên và ở đó cho đến khi user bấm close.
    //  Nếu có nhiều badge unlock cùng lúc → bấm close → hiện cái tiếp theo.
    //
    //  Inspector wiring:
    //    cardPanel — GameObject chứa nội dung card
    //    imgIcon   — Image sprite badge
    //    txtTitle  — TMP_Text tên badge
    //    txtDesc   — TMP_Text mô tả badge
    //    closeBtn  — Button đóng (field của UIBase)
    //    isCache   — tick true
    // ══════════════════════════════════════════════════════════════════════════
    public class UIBadgeUnlock : UIBase
    {
        [Header("Badge Card")]
        public GameObject cardPanel;
        public Image      imgIcon;
        public TMP_Text   txtTitle;
        public TMP_Text   txtDesc;

        private readonly Queue<ESkillId> _queue = new Queue<ESkillId>();

        private bool _initialized = false;

        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            // Chỉ ẩn card lần đầu tiên. Các lần ShowUI sau không reset card đang hiện.
            if (!_initialized)
            {
                if (cardPanel != null) cardPanel.SetActive(false);
                _initialized = true;
            }
        }

        /// <summary>Thêm badge vào queue. Nếu chưa đang show thì show ngay.</summary>
        public void Enqueue(ESkillId id)
        {
            var cfg = SkillConfigDatabase.Instance?.Get(id);
            if (cfg != null && !cfg.showPopupOnUnlock) return;

            _queue.Enqueue(id);

            // Nếu card đang ẩn → show cái đầu tiên trong queue
            if (cardPanel != null && !cardPanel.activeSelf)
                ShowNext();
        }

        /// <summary>User bấm close → show cái tiếp nếu còn, hết thì ẩn popup.</summary>
        public override void OnClickClose()
        {
            HideCard();

            if (_queue.Count > 0)
            {
                ShowNext();
            }
            else
            {
                _initialized = false;
                base.OnClickClose(); // ẩn toàn bộ popup qua UIBase
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
        }

        private void HideCard()
        {
            if (cardPanel != null) cardPanel.SetActive(false);
        }
    }
}
