namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class SelectChangeGameObject : BaseSelect, ISelectBoolCache
    {
        public GameObject[] obSelect; 
        public GameObject[] obUnSelect;
        [SerializeField, ReadOnly]
        private bool _isSelect = false;
        bool ISelectBoolCache.IsSelect => _isSelect;

        public override void Select(bool isSelect)
        {
            int lengthSelect = obSelect.Length;
            int lengthUnSelect = obUnSelect.Length;
            int length = Mathf.Max(lengthSelect, lengthUnSelect);
            for (int i = 0; i < length; i++)
            {
                int index = i;
                if(index < lengthSelect)
                {
                    SetActiveObject(obSelect[index], isSelect);
                }
                if(index < lengthUnSelect)
                {
                    SetActiveObject(obUnSelect[index], !isSelect);
                }
            }
            _isSelect = isSelect;
        }

        void ISelectBoolCache.SelectInvert()
        {
            DoSelectInvert();
        }
        public void DoSelectInvert()
        {
            _isSelect = !_isSelect;
            Select(_isSelect);
        }

        private void SetActiveObject(GameObject ob, bool status)
        {
            if(ob != null)
            {
                ob.SetActive(status);
            }
    
        }
    }

    public interface ISelectBoolCache
    {
        bool IsSelect { get; }
        void SelectInvert();
    }
}
