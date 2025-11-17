using UnityEngine;

public enum ActionResultType
{
    NextStep,
    RepeatStep,
    JumpToStepIndex,
    JumpToStoryboard,
}

public class ActionResult
{
    public ActionResultType Type;
    public object Data;

    public ActionResult(ActionResultType type)
    {
        Type = type;
    }
}