using DG.Tweening;
using Luzart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class StepActionDelayTime : StepAction
{
    private Tween tweener;
    public float delayTime = 1f;    
    public override void Execute(Action<ActionResult> _onComplete)
    {
        base.Execute(_onComplete);
        tweener?.Kill(true);
        UIManager.Instance.BlockRaycast(true);
        tweener = DOVirtual.DelayedCall(delayTime, () =>
        {
            UIManager.Instance.BlockRaycast(false);
            gameObject.SetActive(isSetActiveAfter);
            onComplete?.Invoke(new ActionResult(ActionResultType.NextStep));
        });
    }
    private void OnDisable()
    {
        tweener?.Kill(true);    
    }
    private void OnValidate()
    {
        if (!name.Contains(nameof(StepActionDelayTime)))
            name = nameof(StepActionDelayTime);
    }
}
