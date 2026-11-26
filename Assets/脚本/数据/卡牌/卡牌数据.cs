using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SerializeReferenceEditor;

[CreateAssetMenu(menuName ="Data/Card")]

public class 卡牌数据 : ScriptableObject
{
    [field:SerializeField] public string Description {  get;private set; }//卡片描述
    [field: SerializeField] public int Mana { get; private set; }//能量
    [field: SerializeField] public Sprite Image { get; private set; }//图像
    [field: SerializeField] public string Name { get; private set; }//卡牌名称
    [field: SerializeReference, SR] public Effect ManualTargetEffect { get; private set; } = null;//手动目标效果
    [field: SerializeField]public List<AutoTargetEffect> OtherEffects { get; private set; }//自动目标效果
}
