using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private GameObject damageVFX;
    void OnEnable()
    {
        动作系统.AttachPerformer<DealDamageGA>(DealDamagePerformer);
    }

    void DisEnable()
    {
        动作系统.DetachPerformer<DealDamageGA>();
    }

    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        foreach (var target in dealDamageGA.Targets)
        {
            target.Damage(dealDamageGA.Amount);
            Instantiate(damageVFX, target.transform.position,Quaternion.identity);
            yield return new WaitForSeconds(0.15f);
            if (target.CurrentHealth <= 0)
            {
                if (target is EnemyView enemyView)
                {
                    KillEnemyGA killEnemyGA = new(enemyView);
                    动作系统.Instance.AddReaction(killEnemyGA);
                }
            }
            else
            {

            }
        }
    }
}
