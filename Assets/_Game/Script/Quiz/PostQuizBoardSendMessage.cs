using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Luzart
{
    public class PostQuizBoardSendMessage : PostQuizBoard
    {
        public void OnClickSendString(string str)
        {
            Level4String data = new Level4String();
            data.str = str;
            data.indexQuestion = levelIndex;
            Level4Manager.Instance.AddQuestion(data);
            OnClickNextButton();
            SyncManager.Instance.ForceSyncNow(trigger:  "level_4_force_sync");
        }
    }
}
