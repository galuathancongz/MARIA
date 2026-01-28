using TMPro;
using UnityEngine;

namespace Luzart
{
    public class Item_FoldOut : MonoBehaviour
    {
        public TMP_Text txtTitle;
        public TMP_Text txtContent;

        public void Setup(string title, string content)
        {
            txtTitle.text = title;
            txtContent.text = content;
        }
    }
}
