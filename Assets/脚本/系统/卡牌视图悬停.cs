using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class 卡牌视图悬停 : Singleton<卡牌视图悬停>
{
    [SerializeField] private 卡片视图 cardViewHover;
    public void Show(卡牌 card,Vector3 position)
    {
        cardViewHover.gameObject.SetActive(true);
        cardViewHover.SetUp(card);
        cardViewHover.transform.position = position;

    }
    public void Hide()
    {
        cardViewHover.gameObject.SetActive(false);
    }
}
