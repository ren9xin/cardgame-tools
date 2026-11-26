using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 卡牌 
{
    public string Title => data.Name;
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public Effect ManualTargetEffect=>data.ManualTargetEffect;
    public List<AutoTargetEffect>OtherEffects=>data.OtherEffects;
    public int Mana{ get; private set; }
    private readonly 卡牌数据 data;

    public 卡牌(卡牌数据 cardData)
    {
        data = cardData;
        Mana = cardData.Mana;
    }
}
