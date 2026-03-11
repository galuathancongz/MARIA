using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Luzart
{
    public class Item_ClickToImport : MonoBehaviour
    {
        public EFeedback type;
        public string title;
        public Action<string,EFeedback> onClick;
        public TMP_Text txtType;
        public TMP_Text txtTitle;
        public void Initialize(EFeedback type, string title, Action<string,EFeedback> onClick)
        {
            this.type = type;
            this.title = title;
            this.onClick = onClick;
            txtTitle.text = title;
            txtType.text = type.ToString();
        }
        public void OnClick()
        {
            onClick?.Invoke(title, type);
        }
    }
}
