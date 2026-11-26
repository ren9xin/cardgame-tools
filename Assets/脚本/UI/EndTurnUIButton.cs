using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnUIButton : MonoBehaviour
{
    public void Onclick()
    {
        EnemyTurnGA enemyTurnGA = new();
        动作系统.Instance.Perform(enemyTurnGA);
    }
}
