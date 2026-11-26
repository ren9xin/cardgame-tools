using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="Data/Hero")]

public class HeroData : ScriptableObject
{
    [field:SerializeField] public Sprite Image { get;private set; }
    [field:SerializeField] public int Health { get;private set; }
    [field:SerializeField] public List<卡牌数据> Deck { get;private set; }
    
}
