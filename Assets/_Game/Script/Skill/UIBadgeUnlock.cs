using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Luzart
{
    // ══════════════════════════════════════════════════════════════════════════
    //  UIBadgeUnlock  :  UIBase
    //
    //  Popup thông báo khi người chơi nhận được badge mới.
    //  Được gọi bởi SkillManager.UnlockSkill() thông qua UIManager —
    //  cùng pattern với UIToast:
    //      var popup = UIManager.Instance.ShowUI<UIBadgeUnlock>(UIName.BadgeUnlock);
    //      popup.Enqueue(id);
    //
    //  Inspector wiring:
    //    cardPanel   — GameObject con chứa nội dung card (child của this)
    //    txtEmoji    — emoji lớn  (e.g. "🎓")
    //    txtTitle    — tên badge (auto-localized)
    //    txtDesc     — mô tả badge (auto-localized)
    //    imgIcon     — sprite icon (ẩn tự động nếu null)
    //    imgBg       — background; tint màu theo SkillConfigEntry.badgeColor
    //    closeBtn    — (field của UIBase) nút × đóng sớm
    //
    //    isCache     — PHẢI tick true trong Inspector
    //    uiName      — set = UIName.BadgeUnlock
    // ══════════════════════════════════════════════════════════════════════════
    public class UIBadgeUnlock : UIBase
    {
        // ── Inspector refs ────────────────────────────────────────────────────
        [Header("Badge Card")]
        public GameObject cardPanel;
        public TMP_Text   txtEmoji;
        public TMP_Text   txtTitle;
        public TMP_Text   txtDesc;
        public Image      imgIcon;
        public Image      imgBg;

        [Header("Timing")]
        public float displayDuration  = 3.5f;
        public float gapBetweenPopups = 0.35f;

        // ── Internal queue ────────────────────────────────────────────────────
        private readonly Queue<ESkillId> _queue    = new Queue<ESkillId>();
        private bool                     _isShowing = false;

        // ══════════════════════════════════════════════════════════════════════
        //  UIBase overrides
        // ══════════════════════════════════════════════════════════════════════

        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);          // Setup() + gameObject.SetActive(true)
            if (cardPanel != null) cardPanel.SetActive(false); // card bắt đầu ẩn; Enqueue() mở ra
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Public API  — được gọi bởi SkillManager
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Thêm badge vào hàng đợi để hiển thị.
        /// Nếu badge có showPopupOnUnlock = false trong SkillConfigDatabase thì bỏ qua.
        /// </summary>
        public void Enqueue(ESkillId id)
        {
            var cfg = SkillConfigDatabase.Instance?.Get(id);
            if (cfg != null && !cfg.showPopupOnUnlock) return;

            _queue.Enqueue(id);
            if (!_isShowing)
                StartCoroutine(DrainQueue());
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Queue processing
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator DrainQueue()
        {
            _isShowing = true;

            while (_queue.Count > 0)
            {
                ShowCard(_queue.Dequeue());
                yield return new WaitForSeconds(displayDuration);
                HideCard();
                yield return new WaitForSeconds(gapBetweenPopups);
            }

            _isShowing = false;
        }

        private void ShowCard(ESkillId id)
        {
            var info = SkillDefinition.Get(id);
            var cfg  = SkillConfigDatabase.Instance?.Get(id);

            if (info == null) return;

            if (txtEmoji != null) txtEmoji.text = info.emoji;

            if (txtTitle != null)
                txtTitle.text = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.Get(info.nameKey)
                    : info.nameKey;

            if (txtDesc != null)
                txtDesc.text = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.Get(info.descKey)
                    : info.descKey;

            if (imgIcon != null)
            {
                bool hasSprite = cfg?.icon != null;
                imgIcon.gameObject.SetActive(hasSprite);
                if (hasSprite) imgIcon.sprite = cfg.icon;
            }

            if (imgBg != null && cfg != null)
                imgBg.color = cfg.badgeColor;

            if (cardPanel != null) cardPanel.SetActive(true);
        }

        private void HideCard()
        {
            if (cardPanel != null) cardPanel.SetActive(false);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Close button  (closeBtn field của UIBase — được Setup() tự wire)
        // ══════════════════════════════════════════════════════════════════════

        public override void OnClickClose()
        {
            StopAllCoroutines();
            HideCard();
            _isShowing = false;

            if (_queue.Count > 0)
                StartCoroutine(ResumeAfterGap());
        }

        private IEnumerator ResumeAfterGap()
        {
            yield return new WaitForSeconds(gapBetweenPopups);
            StartCoroutine(DrainQueue());
        }
    }
}
