using Luzart;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ══════════════════════════════════════════════════════════════════════════════
//  UIProfile
//  Shows player info + all skill/badge cards in a single scrollable list.
//
//  Inspector wiring:
//    ── Player Info ──────────────────────────────────────────────────────────
//    txtSubject / txtName / txtAge / bsPersona / btnLogout
//
//    ── Progress ─────────────────────────────────────────────────────────────
//    txtLevelProgress   "Level 2 / 3"
//    txtSkillCount      "6 / 20 skills"
//
//    ── Skill List ───────────────────────────────────────────────────────────
//    skillItemPrefab    UISkillItem prefab (inactive by default)
//    skillListContent   ScrollView → Viewport → Content (Transform)
// ══════════════════════════════════════════════════════════════════════════════
public class UIProfile : UIBase
{
    // ── Player info ──────────────────────────────────────────────────────────
    [Header("Player Info")]
    public TMP_InputField txtSubject;
    public TMP_InputField txtName;
    public TMP_InputField txtAge;
    public BaseSelect     bsPersona;
    public Button         btnLogout;
    public Button         btnExportReport;
    public Button         btnViewFeedback;

    // ── Overall progress ─────────────────────────────────────────────────────
    [Header("Progress")]
    public TMP_Text txtLevelProgress;   // "Level 2 / 3"
    public TMP_Text txtSkillCount;      // "6 / 20 skills"

    // ── Skill list ───────────────────────────────────────────────────────────
    [Header("Skills")]
    public UISkillItem skillItemPrefab;
    public Transform   skillListContent;

    private List<UISkillItem> _skillItems = new List<UISkillItem>();

    // ═════════════════════════════════════════════════════════════════════════
    //  Lifecycle
    // ═════════════════════════════════════════════════════════════════════════

    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        RefreshInfo();
        RefreshSkills();
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Info section
    // ═════════════════════════════════════════════════════════════════════════

    private void RefreshInfo()
    {
        if (txtSubject)
        {
            txtSubject.onEndEdit.RemoveAllListeners();
            txtSubject.onEndEdit.AddListener(SaveSubject);
            txtSubject.text = DataManager.Instance.Data.subjectName;
        }
        if (txtName)
        {
            txtName.onEndEdit.RemoveAllListeners();
            txtName.onEndEdit.AddListener(SaveName);
            if(DataManager.Instance.Data.namePlayer.Equals("username"))
                DataManager.Instance.Data.namePlayer = AuthManager.Instance.CurrentUsername;
            txtName.text = DataManager.Instance.Data.namePlayer;
        }
        if (txtAge)
        {
            txtAge.onEndEdit.RemoveAllListeners();
            txtAge.onEndEdit.AddListener(SaveAge);
            txtAge.text = DataManager.Instance.Data.age.ToString();
        }
        if (bsPersona)
            bsPersona.Select((int)PersonaManager.Instance.GetMyPersonaType());

        GameUtil.ButtonOnClick(btnLogout, OnClickLogout);
        GameUtil.ButtonOnClick(btnExportReport, Luzart.PdfExporter.ExportGrowthReport);
        GameUtil.ButtonOnClick(btnViewFeedback, OnClickViewFeedback);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Skills section
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>Rebuild skill list — only shows unlocked badges.</summary>
    public void RefreshSkills()
    {
        if (skillItemPrefab == null || skillListContent == null) return;

        // Only unlocked skills
        var unlocked = SkillDefinition.GetUnlocked();

        MasterHelper.InitListObj(unlocked.Count, skillItemPrefab, _skillItems, skillListContent,
            (item, i) =>
            {
                item.gameObject.SetActive(true);
                item.Setup(unlocked[i]);
            });

        // Overall counts
        if (SkillManager.Instance != null)
        {
            int count = SkillManager.Instance.CountAll();
            int total = SkillManager.Instance.TotalAll();
            if (txtSkillCount) txtSkillCount.text = $"{count} / {total}";
        }

        // Level progress
        int level = DataManager.Instance != null ? DataManager.Instance.CurrentLevel : 0;
        if (txtLevelProgress) txtLevelProgress.text = $"Level {level} / 3";
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Feedback Summary
    // ═════════════════════════════════════════════════════════════════════════

    private void OnClickViewFeedback()
    {
        UIManager.Instance.ShowUI(UIName.FeedbackSummary);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Logout
    // ═════════════════════════════════════════════════════════════════════════

    private void OnClickLogout()
    {
        if (btnLogout) btnLogout.interactable = false;
        AuthManager.Instance.Logout(() =>
        {
            if (btnLogout) btnLogout.interactable = true;
            UIManager.Instance.HideAll();
            UIManager.Instance.ShowUI(UIName.Login);
        });
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Save helpers
    // ═════════════════════════════════════════════════════════════════════════

    private void SaveSubject(string s)
    {
        DataManager.Instance.Data.subjectName = s;
        DataManager.Instance.SaveGameData();
    }
    private void SaveName(string n)
    {
        DataManager.Instance.Data.namePlayer = n;
        DataManager.Instance.SaveGameData();
    }
    private void SaveAge(string a)
    {
        if (int.TryParse(a, out int age)) DataManager.Instance.Data.age = age;
        else Debug.LogWarning("[UIProfile] Invalid age format");
        DataManager.Instance.SaveGameData();
    }
}
