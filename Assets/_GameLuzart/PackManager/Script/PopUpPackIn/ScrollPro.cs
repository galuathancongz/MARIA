using DG.Tweening;
using Luzart;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollPro : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
    [Header("ParentSpawn")]
    public Transform parentSpawn;
    public ParentSpawnItemPack parentSpawnItemPackPf;
    private List<ParentSpawnItemPack> listParentSpawnItemPacks = new List<ParentSpawnItemPack>();
    [Header("ScrollRect")]
    [SerializeField]
    private ScrollRect _scrollRect = null;
    private ScrollRect scrollRect
    {
        get
        {
            if(_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }
            return _scrollRect;
        }
    }
    [Header("ProgressBar")]
    public Transform parentProgressBar;
    public ProgressBarUI progressBarPf;
    private List<ProgressBarUI> listProgressBarUI = new List<ProgressBarUI>();

    private int _currentItem;
    private Sequence _scrollSequence;

    public float timeWaitToMove = 2.5f;
    public float timeMove = 0.2f;

    public void Initialize(int length, Action<ParentSpawnItemPack, int> onDoneSpawn)
    {
        MasterHelper.InitListObj(length, parentSpawnItemPackPf, listParentSpawnItemPacks, parentSpawn, (item, index) =>
        {
            item.gameObject.SetActive(true);
            item.Initialze(ClickPreScroll, ClickNextScroll);
            
            onDoneSpawn?.Invoke(item, index);

            if(length == 1)
            {
                item.bsHideBtn?.Select(3);
            }
            else if (index == 0)
            {
                item.bsHideBtn?.Select(1);
            }
            else if(index == length - 1)
            {
                item.bsHideBtn?.Select(2);
            }
            else
            {
                item.bsHideBtn?.Select(0);
            }
        });
        MasterHelper.InitListObj(length, progressBarPf, listProgressBarUI, parentProgressBar, (item, index) =>
        {
            item.gameObject.SetActive(true);
        });
        _currentItem = 0;
        if(length > 0)
        {
            OnFirstScroll();
        }

    }
    private void ClickNextScroll()
    {
        StopAutoScroll();
        NextItem();
        FillAmountAnimation();
        ScrollToCurrentItem();
        StartAutoScroll();
    }
    private void ClickPreScroll()
    {
        StopAutoScroll();
        PreItem();
        FillAmountAnimation();
        ScrollToCurrentItem();
        StartAutoScroll();
    }
    private void OnFirstScroll()
    {
        StopAutoScroll();
        _currentItem = 0;
        FillAmountAnimation();
        ScrollToCurrentItem();
        StartAutoScroll();
    }
    public void StartAutoScroll()
    {
        StopAutoScroll();
        _scrollSequence?.Kill(true);
        _scrollSequence = DOTween.Sequence();
        _scrollSequence
            .AppendCallback(FillAmountAnimation)
            .AppendInterval(timeWaitToMove)
            .AppendCallback(NextItem)
            .AppendCallback(ScrollToCurrentItem)
            .AppendInterval(timeMove)
            .SetLoops(-1)
            .SetId(this);
    }

    private void StopAutoScroll()
    {
        if (_scrollSequence != null && _scrollSequence.IsActive())
        {
            _scrollSequence.Kill();
        }
    }

    private void NextItem()
    {
        _currentItem = (int)Mathf.Repeat(_currentItem + 1, listParentSpawnItemPacks.Count);
    }
    private void PreItem()
    {
        _currentItem = (int)Mathf.Repeat(_currentItem - 1, listParentSpawnItemPacks.Count);
    }

    private void ScrollToCurrentItem()
    {
        var parent = scrollRect.content;
        RectTransform targetChild = listParentSpawnItemPacks[_currentItem].GetComponent<RectTransform>();
        float posX = 0;
        for (int i = 0; i < _currentItem; i++)
        {
            posX += listParentSpawnItemPacks[i].GetComponent<RectTransform>().rect.size.x;
        }
        posX = -posX;
        parent.DOAnchorPosX(posX, timeMove).SetEase(Ease.OutQuad).SetId(this);
    }
    private void FillAmountAnimation()
    {
        int length = listProgressBarUI.Count;
        for (int i = 0; i < length; i++)
        {
            int index = i;

            if(_currentItem == index)
            {
                listProgressBarUI[_currentItem].SetSlider(0, 1, timeWaitToMove, null);
            }
            else
            {
                var item = listProgressBarUI[index];
                GameUtil.Instance.StopAllCoroutinesForBehaviour(item);
                item.SetSlider(0, 0, 0, null);
            }
        }

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        StopAutoScroll();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ChangePosition();
    }
    public void ChangePosition()
    {
        _currentItem = FindClosestItem();
        FillAmountAnimation();
        ScrollToCurrentItem();
        StartAutoScroll();
    }

    private int FindClosestItem()
    {
        int closestIndex = 0;
        float minDistance = float.MaxValue;

        for (int i = 0; i < listParentSpawnItemPacks.Count; i++)
        {
            int index = i;
            float distance = Mathf.Abs(scrollRect.content.anchoredPosition.x - GetPos(index));
            if(minDistance > distance)
            {
                minDistance = distance;
                closestIndex = index;
            }

        }

        return closestIndex;
    }
    private float GetPos(int _currentItem)
    {
        float posX = 0;
        for (int i = 0; i < _currentItem; i++)
        {
            posX += listParentSpawnItemPacks[i].GetComponent<RectTransform>().rect.size.x;
        }
        posX = -posX;
        return posX;
    }

    private void OnDisable()
    {
        StopAutoScroll();
        this?.DOKill(true);
    }
}
