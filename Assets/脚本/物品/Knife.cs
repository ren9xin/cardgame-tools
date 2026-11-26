using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : MonoBehaviour
{
    void OnEnable()
    {
        动作系统.SubscribeReaction<DrawCardGA>(DrawCardReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        动作系统.UnsubscribeReaction<DrawCardGA>(DrawCardReaction, ReactionTiming.POST);
    }

    void DrawCardReaction(DrawCardGA drawCardGA)
    {
        //DealDamageGA dealDamageGA = new(3);
        //动作系统.Instance.AddReaction(dealDamageGA);
    }
}
