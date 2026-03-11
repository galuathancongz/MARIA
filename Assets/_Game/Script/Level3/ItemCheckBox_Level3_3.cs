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
        public BaseToggle bsUsing;

        [ContextMenu("Set")]
        private void OnSet()
        {
            var toggles= GetComponentsInChildren<BaseToggle>();
            toggle = toggles.FirstOrDefault(x => x.gameObject.name == "CheckBox");
            bsUsing = toggles.FirstOrDefault(x => x.gameObject.name == "SelectUsing");
            title = GetComponentInChildren<TMPro.TMP_Text>().text;
        }
    }
}
