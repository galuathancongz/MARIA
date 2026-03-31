using UnityEngine;

namespace Luzart
{
    public class Level3_CheckBoxOptionalFilter : MonoBehaviour
    {
        [SerializeField] private int filterIndex;
        [SerializeField] private BaseToggle toggle;

        public void OnClick()
        {
            if (toggle.IsSelect)
                Level3Manager.Instance.Data.AddFilter(filterIndex);
            else
                Level3Manager.Instance.Data.RemoveFilter(filterIndex);
        }

        [ContextMenu("Set")]
        private void OnSet()
        {
            if (toggle == null)
                toggle = GetComponent<BaseToggle>();
        }
    }
}
