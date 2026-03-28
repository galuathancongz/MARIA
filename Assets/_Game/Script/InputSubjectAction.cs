// File: InputSubjectAction.cs
using DG.Tweening;
using Luzart;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputSubjectAction : StepAction
{
    [Header("Subject Input UI Components")]
    public TMP_InputField inputField;     // Sử dụng InputField cho môn học
    public Button btnConfirm;
    public TMP_Text txtInfor;
    [TextArea] public string strInfor;
    public float timeDuration = 1f;
    public Ease easing;

    private Tween _typingTween;

    public override void Execute(Action<ActionResult> _onComplete)
    {
        base.Execute(_onComplete);
        GameUtil.ButtonOnClick(btnConfirm, OnConfirm);
        inputField.onEndEdit.RemoveAllListeners();
        inputField.onEndEdit.AddListener(text =>
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnConfirm();
            }
        });

        // Hiển thị panel và xóa text cũ
        _typingTween = txtInfor.DOText(Loc.T(strInfor), timeDuration).SetEase(easing);
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    public void OnConfirm()
    {
        if (_typingTween != null && _typingTween.IsActive() && _typingTween.IsPlaying())
        {
            // Nếu đang gõ, hoàn tất ngay
            _typingTween.Complete();
            return;
        }
        string subject = inputField.text.Trim();
        if (!TryDetectSubject(subject))
        {
            var ui = UIManager.Instance.ShowUI<UINoti>(UIName.Noti);
            ui.InitPopupFillName();
            return;
        }
        DataManager.Instance.Data.subjectName = subject.ToUpper();
        CallOnComplete();
    }
    private readonly string[] coreSubjects = {
    // --- KHỐI GIÁO DỤC & SƯ PHẠM (Education & Pedagogy) ---
    "pedagogy", "didactics", "educational psychology", "curriculum development",
    "classroom management", "educational technology", "special education",
    "early childhood education", "primary education", "secondary education",
    "higher education", "educational leadership", "school administration",
    "assessment and evaluation", "instructional design", "adult education",
    "inclusive education", "steam education", "tesol", "tefl",
    "physical education pedagogy", "art education", "music education",
    "educational philosophy", "history of education", "comparative education",
    "vocational education", "lifelong learning", "e-learning",

    // --- KHỐI ĐẠI CƯƠNG & CHÍNH TRỊ ---
    "philosophy","marxism-leninism","political economy","scientific socialism",
    "ho chi minh ideology","history of communist party","critical thinking",
    "soft skills","research methodology","introduction to sociology",

    // --- TOÁN & LOGIC ---
    "math","algebra","geometry","calculus","statistics","probability",
    "trigonometry","logic","number theory","linear algebra","discrete math",
    "differential equations","numerical analysis","optimization",

    // --- VẬT LÝ ---
    "physics","mechanics","thermodynamics","optics","electromagnetism",
    "quantum physics","nuclear physics","astrophysics","relativity",

    // --- HÓA HỌC ---
    "chemistry","organic chemistry","inorganic chemistry","physical chemistry",
    "analytical chemistry","biochemistry","materials chemistry","chemical engineering",

    // --- SINH HỌC ---
    "biology","cell biology","genetics","microbiology","ecology",
    "evolution","zoology","botany","anatomy","physiology","neuroscience",

    // --- KỸ THUẬT & CÔNG NGHỆ ---
    "materials","material science","metallurgy","mechanical engineering",
    "civil engineering","electrical engineering","electronics","mechatronics",
    "robotics","aerospace engineering","control engineering",

    // --- CÔNG NGHỆ THÔNG TIN (IT) ---
    "computer","computer science","programming","coding","software",
    "algorithms","data structures","databases","web development",
    "game development","artificial intelligence","machine learning",
    "data science","networking","cyber security","cloud computing",

    // --- KINH TẾ & QUẢN TRỊ ---
    "economics","microeconomics","macroeconomics","econometrics",
    "business","management","marketing","finance","accounting","audit",
    "entrepreneurship","human resources","supply chain","logistics",

    // --- NGÔN NGỮ & VĂN HÓA ---
    "english","literature","linguistics","languages","translation",
    "french","spanish","german","chinese","japanese","vietnamese",

    // --- LUẬT & CHÍNH TRỊ ---
    "law","ethics","jurisprudence","international law","civil law",
    "criminal law","constitutional law","politics","international relations",

    // --- Y DƯỢC & SỨC KHỎE ---
    "medicine","pharmacy","nursing","dentistry","public health",
    "pharmacology","pathology","epidemiology","surgery",

    // --- KIẾN TRÚC & THIẾT KẾ ---
    "art","music","drawing","painting","animation","film","photography",
    "architecture","graphic design","fashion design","urban planning",

    // --- TÂM LÝ & XÃ HỘI ---
    "psychology","sociology","anthropology","archaeology","geography",
    "history","gender studies","social work",

    // --- MÔI TRƯỜNG & TRÁI ĐẤT ---
    "environmental science","geology","climate science","oceanography",
    "meteorology","hydrology","forestry","agriculture",


    "science","literature","history","art","music","philosophy","economics",
};
    SubjectDetector detector = null;
    private bool TryDetectSubject(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;
        if(detector == null)
        {
            detector = new SubjectDetector(coreSubjects);
        }
        input = input.Trim().ToLowerInvariant();
        return detector.TryDetectSubject(input);
    }

public class SubjectDetector
{
    private readonly HashSet<string> _subjectHash;
    private readonly Regex _combinedRegex;

    public SubjectDetector(string[] subjects)
    {
        // 1. Sắp xếp từ dài nhất đến ngắn nhất để ưu tiên khớp cụm từ đầy đủ
        var sortedSubjects = subjects
            .OrderByDescending(s => s.Length)
            .Select(Regex.Escape) // Tránh lỗi nếu môn học có ký tự đặc biệt như C++
            .ToArray();

        // 2. Tạo Regex tổng hợp với Word Boundary (\b) để khớp chính xác nguyên từ
        // Ví dụ: \b(organic chemistry|chemistry|math)\b
        string pattern = @"(" + string.Join("|", sortedSubjects) + @")";
        _combinedRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // 3. Lưu vào Hashset để tra cứu O(1) nếu cần kiểm tra khớp hoàn toàn
        _subjectHash = new HashSet<string>(subjects, StringComparer.OrdinalIgnoreCase);
    }

    public bool TryDetectSubject(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        // Cách này cực nhanh vì Regex Compiled quét chuỗi 1 lần duy nhất
        return _combinedRegex.IsMatch(input);
    }

    public string GetDetectedSubjectName(string input)
    {
        var match = _combinedRegex.Match(input);
        return match.Success ? match.Value : null;
    }
}
}
