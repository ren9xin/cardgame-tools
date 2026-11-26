using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCardGA : GameAction
{
    public EnemyView ManualTarget { get; private set; }
    public 卡牌 Card { get; set; }
    public PlayCardGA(卡牌 card)
    {
        ManualTarget=null;
        Card = card;
    }

    public PlayCardGA(卡牌 card,EnemyView target)
    {
        Card = card;
        ManualTarget = target;
    }
}
