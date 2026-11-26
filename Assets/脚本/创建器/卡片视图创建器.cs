using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 卡片视图创建器 : Singleton<卡片视图创建器>
{
    [SerializeField] private 卡片视图 cardViewPrefab;
    public 卡片视图 CreateCardView(卡牌 card,Vector3 position,Quaternion rotation)
    {
        卡片视图 cardView=Instantiate(cardViewPrefab,position,rotation);
        cardView.transform.localScale = Vector3.zero ;
        cardView.transform.DOScale(Vector3.one, 0.15f);
        cardView.SetUp(card);
        return cardView;
    }
}
