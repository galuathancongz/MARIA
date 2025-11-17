using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAnimationUI : MonoBehaviour
{
    public Animator animator;
    public bool isOpen = false;

    public void OnClickAnimation()
    {
        isOpen = !isOpen;
        if (isOpen)
        {
            animator.Play("BtnOut");
            transform.DORotate(new Vector3(0, 0, 180), 0.2f);
        }
        else
        {
            animator.Play("BtnIn");
            transform.DORotate(new Vector3(0, 0, 0), 0.2f);
        }

    }
}
