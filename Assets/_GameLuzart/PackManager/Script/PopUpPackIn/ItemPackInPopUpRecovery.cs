using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPackInPopUpRecovery : ItemPackInPopUp
{

    protected override void OnCompleteBuy()
    {
        base.OnCompleteBuy();
    }
    protected override void OnDoneReceivePopUp()
    {
        base.OnDoneReceivePopUp();
        ContinueGame();
    }
    private void ContinueGame()
    {

    }
}
