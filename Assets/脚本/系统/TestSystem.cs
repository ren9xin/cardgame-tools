using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSystem : MonoBehaviour
{
    [SerializeField] private List<卡牌数据> deckData;
    private void Start()
    {
        CardSystem.Instance.Setup(deckData);
    }
    
}
