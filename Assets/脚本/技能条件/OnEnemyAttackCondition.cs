using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OnEnemyAttackCondition : PerkCondition
{
    public override bool SubConditionIsMet(GameAction gameAction)
    {
        return true;
    }

    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        动作系统.SubscribeReaction<AttackHeroGA>(reaction,reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        动作系统.UnsubscribeReaction<AttackHeroGA>(reaction, reactionTiming);
    }
    
}
