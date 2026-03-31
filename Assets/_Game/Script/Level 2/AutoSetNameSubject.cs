using Luzart;
using TMPro;
using UnityEngine;

public class AutoSetNameSubject : MonoBehaviour
{
    public TMP_Text txt;
    public int level = 3;
    [SerializeField][ReadOnly]ESubject _subject;
    private void OnEnable()
    {
        if (level == 2)
        {
            _subject = Level2Manager.Instance.Data.subject;

        }
        else if (level == 3)
        {
            _subject = Level3Manager.Instance.Data.subject;
        }
        string str = MentorSubjectExtension.GetSubjectName(_subject);
        if(txt)
        {
            txt.text = str;
        }
    }
    private void OnValidate()
    {
        txt = GetComponent<TMP_Text>();
    }
}
