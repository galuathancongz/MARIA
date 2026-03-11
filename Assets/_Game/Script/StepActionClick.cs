using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepActionClick : StepAction
{
    public enum Mode
    {
        Button = 0,
        Mouse = 1
    } 
    public Mode mode = Mode.Button;
    public void OnClickOnDone()
    {
        CallOnComplete();
    }
    private void Update()
    {
        if(mode == Mode.Mouse)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnClickOnDone();
            }
        }   
    }
}
