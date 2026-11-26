using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseStatsGA : MonoBehaviour
{
    public Minion Target;
    public int AttackIncreaseAmount;
    public int HealthIncreaseAmount;
    public IncreaseStatsGA(Minion Target, int attackIncreaseAmount,int healthIncreaseAmount)
    {
        Target = Target;
        AttackIncreaseAmount = attackIncreaseAmount;
        HealthIncreaseAmount = healthIncreaseAmount;
    }
}
