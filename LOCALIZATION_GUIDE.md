# MARIA Teaching Classroom - Localization Guide

## Tong Quan

He thong Localization ho tro 2 ngon ngu: **English (en)** va **Vietnamese (vi)**.
Tat ca text UI + AI prompt deu duoc quan ly tap trung trong 2 file JSON.

---

## Cau Truc

```
Assets/_GameLuzart/Localization/
├── Script/
│   ├── LocalizationManager.cs    ← Singleton quan ly ngon ngu
│   ├── LocalizedText.cs          ← Component gan vao TMP_Text
│
└── Resources/Localization/
    ├── en.json                   ← 60+ keys tieng Anh
    └── vi.json                   ← 60+ keys tieng Viet
```

---

## Setup Trong Unity

### 1. Them LocalizationManager vao Scene

Trong `Game.unity`, tao **Empty GameObject** ten `LocalizationManager`:
- Add component: **LocalizationManager**
- Dat Script Execution Order: **-200** (truoc tat ca cac manager khac)

### 2. Su dung trong Code

```csharp
// Lay text don gian
string text = LocalizationManager.Instance.Get("ui.error_try_again");

// Lay text co format {0}, {1}
string text = LocalizationManager.Instance.GetFormat("ui.unlock_booster", 5);

// Lay prompt co named placeholder {mentorName}, {subject}
string prompt = LocalizationManager.Instance.GetPrompt("prompts.level2_2_conversation",
    new Dictionary<string, string> {
        {"mentorName", "Austen"},
        {"subjectName", "English"},
        {"question", userQuestion}
    });

// Doi ngon ngu
LocalizationManager.Instance.SetLanguage("vi");  // Chuyen sang tieng Viet
LocalizationManager.Instance.SetLanguage("en");  // Chuyen sang tieng Anh

// Kiem tra ngon ngu hien tai
string lang = LocalizationManager.Instance.CurrentLanguage; // "en" hoac "vi"
```

### 3. Su dung LocalizedText cho UI Static

Gan component `LocalizedText` vao bat ky **TMP_Text** nao:
1. Chon GameObject co TMP_Text
2. Add Component > **LocalizedText**
3. Dien `Loc Key` = key trong JSON (vd: `ui.fill_subject`)
4. Text se tu dong cap nhat khi doi ngon ngu

### 4. Lang nghe su kien doi ngon ngu

```csharp
// Cach 1: Observer
Observer.Instance.AddObserver(ObserverKey.OnLanguageChanged, (data) => {
    // Cap nhat UI tai day
});

// Cach 2: Action
LocalizationManager.Instance.OnLanguageChanged += () => {
    // Cap nhat UI tai day
};
```

---

## Danh Sach Key

### UI Keys (`ui.*`)
| Key | Mo ta |
|-----|-------|
| `ui.offline` | Thong bao mat mang |
| `ui.expansion` | Phien ban mo rong |
| `ui.unlock_booster` | Mo khoa booster (co {0} = level) |
| `ui.unlock_previous` | Yeu cau mo khoa truoc |
| `ui.fill_subject` | Yeu cau nhap mon hoc |
| `ui.correct_prompt` | Nhap dung prompt |
| `ui.wait_ai_refine` | Doi AI hoan thanh |
| `ui.wait_ai_complete` | Doi AI tra loi |
| `ui.completed_all` | Hoan thanh tat ca |
| `ui.error_invalid_data` | Loi du lieu |
| `ui.error_parse_feedback` | Loi phan tich feedback |
| `ui.generating_feedback` | Dang tao feedback |
| `ui.error_loading` | Loi tai du lieu |
| `ui.error_try_again` | Loi chung |
| `ui.error_parse_response` | Loi phan tich |
| `ui.loading` | Dang tai |
| `ui.start_level` | Bat dau level (co {0}) |
| `ui.default_prompt` | Prompt mac dinh tutorial |
| `ui.error_parse_student` | Loi phan tich bai hoc sinh |
| `ui.student_response` | Phan hoi hoc sinh (co {0}) |
| `ui.generating_suggestion` | Dang tao goi y |
| `ui.regenerate_request` | Yeu cau tao lai |

### Mentor Keys (`mentor.*`)
| Key | Mo ta |
|-----|-------|
| `mentor.name.english/math/history/science/default` | Ten mentor theo mon |
| `mentor.subject.english/math/history/science/default` | Ten mon hoc |

### Persona Keys (`persona.*`)
| Key | Mo ta |
|-----|-------|
| `persona.creative/logical/empathetic/unknown` | Loai persona |
| `persona.color.orange/blue/green/black` | Mau persona |

### Prompt Keys (`prompts.*`)
| Key | Placeholders |
|-----|-------------|
| `prompts.level2_start` | (khong co) |
| `prompts.level2_1_context` | {mentorName}, {subjectName} |
| `prompts.level2_2_conversation` | {mentorName}, {subjectName}, {question} |
| `prompts.level2_2_1_question` | {question} |
| `prompts.level2_3_1_request` | {mentorName}, {userRequest} |
| `prompts.level2_3_1_refine` | {mentorName}, {userRequest}, {refinement} |
| `prompts.level2_3_1_regenerate` | {mentorName}, {userRequest} |
| `prompts.level2_4_summary` | (khong co) |
| `prompts.level3_start` | (khong co) |
| `prompts.level3_3_lesson` | {topic}, {baseObjective}, {constraints}, {filters}, {currentField}, {userRequest} |
| `prompts.level3_4_feedback` | {fullContent} |
| `prompts.level3_5_student_work` | {subject}, {topic}, {objective}, {lessonContent}, {studentName}, {studentStyle} |
| `prompts.level3_5_suggestions` | {subject}, {studentWork}, {objective} |
| `prompts.level3_7_final` | {subject}, {topic}, {revisionCount}, {usedInclusion}, {studentWork} |

---

## Them Ngon Ngu Moi

1. Copy `en.json` thanh `xx.json` (vd: `fr.json` cho tieng Phap)
2. Dich tat ca `value` trong file moi
3. Dat file vao `Assets/_GameLuzart/Localization/Resources/Localization/`
4. Goi `LocalizationManager.Instance.SetLanguage("fr")`

---

## Danh Sach File Da Sua

| # | File | Thay doi |
|---|------|----------|
| 1 | Observer.cs | Them `OnLanguageChanged` key |
| 2 | UIToast.cs | 4 const → LocalizationManager.Get() |
| 3 | UINoti.cs | strFillName → LocalizationManager.Get() |
| 4 | Level2Manager.cs | MentorSubjectExtension → localization keys |
| 5 | PersonaManager.cs | Persona names + colors → localization keys |
| 6 | Level2_StartPostRequest.cs | Prompt → localization |
| 7 | Level2_1_SelectLogicStepAction.cs | Context prompt → localization |
| 8 | Level2_2_Conversation.cs | Conversation prompt → localization |
| 9 | Level2_2_1_Logic.cs | Question prefix → localization |
| 10 | UIStoryboard_Level2_3_1.cs | 3 prompts + 3 error msgs → localization |
| 11 | UIStoryboard_Level2_4.cs | Summary prompt → localization |
| 12 | Level3_StartPostRequest.cs | Start prompt → localization |
| 13 | UIStoryboard_Level3_3.cs | Lesson prompt + 4 UI msgs → localization |
| 14 | UIStoryboard_Level3_4.cs | Feedback prompt + error msg → localization |
| 15 | UIStoryboard_Level3_5.cs | 2 prompts + 6 UI msgs → localization |
| 16 | UIStoryboard_Level3_7.cs | Final prompt + 2 error msgs → localization |

**File Moi:**
| File | Mo ta |
|------|-------|
| LocalizationManager.cs | Singleton load/switch ngon ngu |
| LocalizedText.cs | Component cho TMP_Text |
| en.json | 60+ keys tieng Anh |
| vi.json | 60+ keys tieng Viet |
