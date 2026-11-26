using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using System.Linq;

public class 手牌视图 : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;//卡牌曲线
    private readonly List<卡片视图> cards = new();

    public IEnumerator AddCard(卡片视图 CardView)
    {
        cards.Add(CardView);
        yield return UpdateCardPositions(0.15f);
    }

    public 卡片视图 RemoveCard(卡牌 card)
    {
        卡片视图 cardView = GetCardView(card);
        if(cardView == null) return null;
        cards.Remove(cardView);
        StartCoroutine(UpdateCardPositions(0.15f));
        return cardView;
    }

    private 卡片视图 GetCardView(卡牌 card)
    {
        return cards.Where(cardView=>cardView.Card==card).FirstOrDefault();
    }
    private IEnumerator UpdateCardPositions(float duration)
    {
        if (cards.Count == 0) yield break;
        float cardSpacing = 1f / 10f;
        float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2;
        Spline spline = splineContainer.Spline;
        for (int i = 0; i < cards.Count; i++)
        {
            float p = firstCardPosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(p);
            Vector3 forward = spline.EvaluateTangent(p);//切线向量
            Vector3 up = spline.EvaluateUpVector(p);//上方向向量
            Quaternion rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            cards[i].transform.DOMove(splinePosition + transform.position + 0.01f * i * Vector3.back, duration);
            cards[i].transform.DORotate(rotation.eulerAngles, duration);
        }
        yield return new WaitForSeconds(duration);
    }



}
