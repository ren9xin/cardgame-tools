using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaSystem : Singleton<ManaSystem>
{
    [SerializeField] private ManaUI manaUI;
    private const int MAX_MANA = 5;
    private int currentMana =MAX_MANA;

    void OnEnable()
    {
        动作系统.AttachPerformer<SpendManaGA>(SpendManaPerformer);
        动作系统.AttachPerformer<RefillManaGA>(RefillManaPerformer);
    }

    void DisEnable()
    {
        动作系统.DetachPerformer<SpendManaGA>();
        动作系统.DetachPerformer<RefillManaGA>();
    }

    public bool HasEnoughMana(int mana)
    {
        return currentMana>=mana;
    }

    private IEnumerator SpendManaPerformer(SpendManaGA spendManaGA)
    {
        currentMana -= spendManaGA.Amount;
        manaUI.UpdateManaText(currentMana);
        yield return null;
    }

    private IEnumerator RefillManaPerformer(RefillManaGA refillManaGA)
    {
        currentMana=MAX_MANA;
        manaUI.UpdateManaText(currentMana);
        yield return null;
    }
}
