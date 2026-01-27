using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Luzart
{
    public class ListDataResponeItem : MonoBehaviour
    {
        public Transform content;
        public List<Level2_ConversationItem> listItem = new List<Level2_ConversationItem>();
        public Level2_ConversationItem  conversationItem;

        public void SetupData(List<string> data)
        {
            MasterHelper.InitListObj(data.Count, conversationItem, listItem , content, (item, index) =>
            {
                item.gameObject.SetActive(true);
                item.ShowText(data[index]);
            });
        }
        public void SetupDataShowTextAnim(List<string> data)
        {
            MasterHelper.InitListObj(data.Count, conversationItem, listItem, content, (item, index) =>
            {
                item.gameObject.SetActive(true);
                item.ShowTextAnim(data[index]);
            });
        }
        public void SetupDataThinking()
        {
            MasterHelper.InitListObj(1, conversationItem, listItem, content, (item, index) =>
            {
                item.gameObject.SetActive(true);
                item.SetLoading();
            });
        }
    }
}
