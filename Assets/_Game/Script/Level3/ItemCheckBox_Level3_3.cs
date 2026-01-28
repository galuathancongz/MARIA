using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class ItemCheckBox_Level3_3 : MonoBehaviour
    {
        public string title;
        public BaseToggle toggle;

        [ContextMenu("Set")]
        private void OnSet()
        {
            toggle= GetComponentInChildren<BaseToggle>();
            title = GetComponentInChildren<TMPro.TMP_Text>().text;
        }
    }
}
