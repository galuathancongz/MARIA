using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Luzart
{
    public class Level3_CheckBoxOptionalFilter : MonoBehaviour
    {
        [SerializeField] private string filterName;
        [SerializeField] private BaseToggle toggle;
        public void OnClick()
        {
            if (toggle.IsSelect)
            {
                Level3Manager.Instance.Data.AddFilter(filterName);
            }
            else
            {
                Level3Manager.Instance.Data.RemoveFilter(filterName);
            }
            Level3Manager.Instance.Save();
        }
        [ContextMenu("Set")]
        private void OnSet()
        {
            if (toggle == null)
            {
                toggle = GetComponent<BaseToggle>();
            }
            filterName = transform.parent.GetComponentInChildren<TMP_Text>().text;
        }
    }
}
