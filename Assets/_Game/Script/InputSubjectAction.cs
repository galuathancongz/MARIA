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
    private readonly string[] coreSubjectsEN = {
    // --- Education & Pedagogy ---
    "pedagogy", "didactics", "educational psychology", "curriculum development",
    "classroom management", "educational technology", "special education",
    "early childhood education", "primary education", "secondary education",
    "higher education", "educational leadership", "school administration",
    "assessment and evaluation", "instructional design", "adult education",
    "inclusive education", "steam education", "tesol", "tefl",
    "physical education pedagogy", "art education", "music education",
    "educational philosophy", "history of education", "comparative education",
    "vocational education", "lifelong learning", "e-learning",

    // --- General & Politics ---
    "philosophy","marxism-leninism","political economy","scientific socialism",
    "ho chi minh ideology","history of communist party","critical thinking",
    "soft skills","research methodology","introduction to sociology",

    // --- Math & Logic ---
    "math","mathematics","algebra","geometry","calculus","statistics","probability",
    "trigonometry","logic","number theory","linear algebra","discrete math",
    "differential equations","numerical analysis","optimization",

    // --- Physics ---
    "physics","mechanics","thermodynamics","optics","electromagnetism",
    "quantum physics","nuclear physics","astrophysics","relativity",

    // --- Chemistry ---
    "chemistry","organic chemistry","inorganic chemistry","physical chemistry",
    "analytical chemistry","biochemistry","materials chemistry","chemical engineering",

    // --- Biology ---
    "biology","cell biology","genetics","microbiology","ecology",
    "evolution","zoology","botany","anatomy","physiology","neuroscience",

    // --- Engineering & Technology ---
    "materials","material science","metallurgy","mechanical engineering",
    "civil engineering","electrical engineering","electronics","mechatronics",
    "robotics","aerospace engineering","control engineering",

    // --- IT ---
    "computer","computer science","programming","coding","software",
    "algorithms","data structures","databases","web development",
    "game development","artificial intelligence","machine learning",
    "data science","networking","cyber security","cloud computing",

    // --- Economics & Business ---
    "economics","microeconomics","macroeconomics","econometrics",
    "business","management","marketing","finance","accounting","audit",
    "entrepreneurship","human resources","supply chain","logistics",

    // --- Languages & Culture ---
    "english","literature","linguistics","languages","translation",
    "french","spanish","german","chinese","japanese","vietnamese",

    // --- Law & Politics ---
    "law","ethics","jurisprudence","international law","civil law",
    "criminal law","constitutional law","politics","international relations",

    // --- Medicine & Health ---
    "medicine","pharmacy","nursing","dentistry","public health",
    "pharmacology","pathology","epidemiology","surgery",

    // --- Architecture & Design ---
    "art","music","drawing","painting","animation","film","photography",
    "architecture","graphic design","fashion design","urban planning",

    // --- Psychology & Social ---
    "psychology","sociology","anthropology","archaeology","geography",
    "history","gender studies","social work",

    // --- Environment & Earth ---
    "environmental science","geology","climate science","oceanography",
    "meteorology","hydrology","forestry","agriculture",

    "science",
    };

    private readonly string[] coreSubjectsVI = {
    // --- Giáo dục ---
    "sư phạm", "giáo dục", "tâm lý giáo dục", "phát triển chương trình",
    "quản lý lớp học", "công nghệ giáo dục", "giáo dục đặc biệt",
    "giáo dục mầm non", "giáo dục tiểu học", "giáo dục trung học",
    "giáo dục đại học",

    // --- Toán ---
    "toán", "toán học", "đại số", "hình học", "giải tích", "thống kê",
    "xác suất", "lượng giác", "số học", "đại số tuyến tính",

    // --- Vật lý ---
    "vật lý", "cơ học", "nhiệt động lực học", "quang học", "điện từ",
    "vật lý lượng tử", "vật lý hạt nhân", "thiên văn",

    // --- Hóa học ---
    "hóa học", "hóa hữu cơ", "hóa vô cơ", "hóa lý", "hóa phân tích",
    "sinh hóa",

    // --- Sinh học ---
    "sinh học", "sinh học tế bào", "di truyền học", "vi sinh", "sinh thái",
    "tiến hóa", "động vật học", "thực vật học", "giải phẫu", "sinh lý",
    "khoa học thần kinh",

    // --- Kỹ thuật & CNTT ---
    "kỹ thuật", "cơ khí", "xây dựng", "điện", "điện tử", "cơ điện tử",
    "robot", "tin học", "khoa học máy tính", "lập trình", "phần mềm",
    "trí tuệ nhân tạo", "học máy", "khoa học dữ liệu", "an ninh mạng",

    // --- Kinh tế ---
    "kinh tế", "kinh tế vi mô", "kinh tế vĩ mô", "kinh doanh",
    "quản trị", "marketing", "tài chính", "kế toán", "kiểm toán",
    "nhân sự", "logistics",

    // --- Ngôn ngữ ---
    "tiếng anh", "văn học", "ngôn ngữ học", "ngôn ngữ", "dịch thuật",
    "tiếng pháp", "tiếng tây ban nha", "tiếng đức", "tiếng trung",
    "tiếng nhật", "tiếng việt", "ngữ văn",

    // --- Luật ---
    "luật", "đạo đức", "luật quốc tế", "luật dân sự", "luật hình sự",
    "chính trị", "quan hệ quốc tế",

    // --- Y dược ---
    "y học", "dược", "điều dưỡng", "nha khoa", "y tế công cộng",

    // --- Nghệ thuật & Thiết kế ---
    "mỹ thuật", "âm nhạc", "hội họa", "vẽ", "hoạt hình", "phim",
    "nhiếp ảnh", "kiến trúc", "thiết kế đồ họa", "thiết kế thời trang",

    // --- Tâm lý & Xã hội ---
    "tâm lý", "tâm lý học", "xã hội học", "nhân chủng học", "khảo cổ",
    "địa lý", "lịch sử", "công tác xã hội",

    // --- Môi trường ---
    "khoa học môi trường", "địa chất", "khí hậu", "hải dương học",
    "khí tượng", "nông nghiệp", "lâm nghiệp",

    // --- Tổng hợp ---
    "khoa học", "triết học", "nghệ thuật",
    };

    SubjectDetector detector = null;
    private bool TryDetectSubject(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;
        if (detector == null)
        {
            // Merge cả EN + VI để detect được cả 2 ngôn ngữ
            var all = new string[coreSubjectsEN.Length + coreSubjectsVI.Length];
            coreSubjectsEN.CopyTo(all, 0);
            coreSubjectsVI.CopyTo(all, coreSubjectsEN.Length);
            detector = new SubjectDetector(all);
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
