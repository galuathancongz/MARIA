namespace Luzart
{
    //using System;
    //using UnityEngine;
    //using UnityEngine.UI;
    //using DG.Tweening;
    
    //public class SpawningAndMoveToTarget : MonoBehaviour
    //{
    
    //    public RectTransform coinPf;
    //    public Transform target;
    
    //    public int amount = 10;
    
    //    [Space, Header("Jump")]
    //    public float powerJump = 1f;
    //    public int numJump = 1;
    
    //    [Space, Header("Time")]
    //    public float timeMin = 0.4f, timeMax = 0.6f;
    
    //    public void StartMove(Action onFirstTimeMoveTo = null, Action onCompleteMoveTo = null)
    //    {
    //        int _coinAmount = amount;
    //        bool isShowPtc = false;
    //        isShowPtc = false;
    //        //AudioManager.Instance.PlaySFXRewardCoin();
    //        for (int i = 0; i < _coinAmount; i++)
    //        {
    //            DOVirtual.DelayedCall(UnityEngine.Random.Range(0, .4f), () => CallBack(onFirstTimeMoveTo, onCompleteMoveTo));
    //        }
    
    //        void CallBack(Action onFirstTimeMoveTo, Action onCompleteMoveTo)
    //        {
    //            Sequence sequence = DOTween.Sequence();
    //            var coin = Instantiate<RectTransform>(coinPf, transform);
    //            coin.gameObject.SetActive(true);
    //            coin.transform.localScale = new Vector2(0f, 0f);
    //            sequence.Append(coin.transform.DOScale(UnityEngine.Random.Range(1.05f, 1.3f), UnityEngine.Random.Range(.1f, .2f)));
    //            sequence.Append(coin.transform.DOScale(UnityEngine.Random.Range(.6f, .9f), UnityEngine.Random.Range(.1f, .2f)));
    //            sequence.Append(coin.transform.DOScale(1f, UnityEngine.Random.Range(.1f, .2f)));
    //            sequence.AppendInterval(UnityEngine.Random.Range(.1f, .3f));
    //            sequence.Append(coin.DOMove(target.position, UnityEngine.Random.Range(.4f, .6f)).SetEase(Ease.InBack).OnComplete(() =>
    //            {
    //                _coinAmount--;
    //                if (_coinAmount == 0)
    //                {
    //                    onCompleteMoveTo?.Invoke();
    //                }
    //                if (!isShowPtc)
    //                {
    //                    onFirstTimeMoveTo?.Invoke();
    //                    isShowPtc = true;
    //                }
    //                Destroy(coin.gameObject);
    //            }));
    //            sequence.Insert(0, coin.DOAnchorPos(new Vector2(UnityEngine.Random.Range(-110, 110), UnityEngine.Random.Range(-110, 110)), UnityEngine.Random.Range(.1f, .3f)));
    //            sequence.AppendCallback(() => Observer.Instance.Notify(ObserverKey.CoinObserverDontAuto, false));
    //            sequence.AppendCallback(() => Observer.Instance.Notify(ObserverKey.CoinObserverNormal));
    //            sequence.SetTarget(this);
    //        }
    //    }
    //    public Ease easeMove = Ease.Linear;
    //    public Tween StartMoveTween()
    //    {
    //        int _coinAmount = amount;
    //        Sequence sequence = DOTween.Sequence();
    //        for (int i = 0; i < _coinAmount; i++)
    //        {
    //            float timeInsert = UnityEngine.Random.Range(0, 0.4f);
    //            sequence.Insert(timeInsert, CallBack());
    //        }
    //        return sequence;
    //        Tween CallBack()
    //        {
    //            Sequence sq = DOTween.Sequence();
    //            var coin = Instantiate(coinPf, transform);
    //            coin.gameObject.SetActive(true);
    //            coin.transform.localScale = new Vector2(0f, 0f);
    //            sq.Append(coin.transform.DOScale(UnityEngine.Random.Range(1.05f, 1.3f), UnityEngine.Random.Range(.1f, .2f)));
    //            sq.Append(coin.transform.DOScale(UnityEngine.Random.Range(.6f, .9f), UnityEngine.Random.Range(.1f, .2f)));
    //            sq.Append(coin.transform.DOScale(1f, UnityEngine.Random.Range(.1f, .2f)));
    //            sq.Append(coin.DOMove(target.position, UnityEngine.Random.Range(.4f, .6f)).SetEase(easeMove));
    //            sq.Insert(0, coin.DOAnchorPos(new Vector2(UnityEngine.Random.Range(-110, 110), UnityEngine.Random.Range(-110, 110)), UnityEngine.Random.Range(.1f, .3f)));
    //            sq.OnComplete(() =>
    //            {
    //                Destroy(coin.gameObject);
    //            });
    //            sq.SetTarget(this);
    //            return sq;
    //        }
    //    }
    //    public Ease easeJump = Ease.Linear;
    //    public Tween StartMoveJumpTween()
    //    {
    //        int _coinAmount = amount;
    //        Sequence sequence = DOTween.Sequence();
    //        for (int i = 0; i < _coinAmount; i++)
    //        {
    //            float timeInsert = UnityEngine.Random.Range(0, 0.4f);
    //            sequence.Insert(timeInsert, CallBack());
    //        }
    //        return sequence;
    //        Tween CallBack()
    //        {
    //            Sequence sq = DOTween.Sequence();
    //            var coin = Instantiate(coinPf, transform);
    //            coin.gameObject.SetActive(true);
    //            coin.transform.localScale = new Vector2(0f, 0f);
    //            sq.Append(coin.transform.DOScale(UnityEngine.Random.Range(1.05f, 1.3f), UnityEngine.Random.Range(.1f, .2f)));
    //            sq.Append(coin.transform.DOScale(UnityEngine.Random.Range(.6f, .9f), UnityEngine.Random.Range(.1f, .2f)));
    //            sq.Append(coin.transform.DOScale(1f, UnityEngine.Random.Range(.1f, .2f)));
    //            sq.Append(coin.DOJump(target.position,powerJump,numJump, UnityEngine.Random.Range(timeMin, timeMax)).SetEase(easeJump));
    //            sq.Insert(0, coin.DOAnchorPos(new Vector2(UnityEngine.Random.Range(-110, 110), UnityEngine.Random.Range(-110, 110)), UnityEngine.Random.Range(.1f, .3f)));
    //            sq.OnComplete(() =>
    //            {
    //                Destroy(coin.gameObject);
    //            });
    //            sq.SetTarget(this);
    //            return sq;
    //        }
    //    }
    
    //    [Space, Header("Option ResUI")]
    //    public ResUI resUI;
    //    private DataResource dataRes;
    //    public void InitializeResUI(DataResource dataRes)
    //    {
    //        this.dataRes = dataRes; 
    //        resUI.InitData(dataRes);
    //    }
    //    private void OnDestroy()
    //    {
    //        this.DOKill();
    //    }
    //}
}
