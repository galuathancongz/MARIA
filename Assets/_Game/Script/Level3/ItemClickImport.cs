using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Luzart
{
    public class ItemClickImport : MonoBehaviour
    {
        public string title;
        public Action<string> onClick;
        public UnityEvent<string> onClickEvent;
        public void Initialize( Action<string> onClick)
        {
            
            this.onClick = onClick;
        }
        public void OnClick()
        {
            onClick?.Invoke(title);
            onClickEvent?.Invoke(title);
        }

        private void OnValidate()
        {
            title = GetComponentInChildren<TMP_Text>()?.text;
        }
    }
}
