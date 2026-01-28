using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Luzart
{
    public class ItemClickImport : MonoBehaviour
    {
        public string title;
        public Action<string> onClick;
        public void Initialize( Action<string> onClick)
        {
            
            this.onClick = onClick;
        }
        public void OnClick()
        {
            onClick?.Invoke(title);
        }

        private void OnValidate()
        {
            title = GetComponentInChildren<TMP_Text>()?.text;
        }
    }
}
