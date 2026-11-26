using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class CardSystem : Singleton<CardSystem>
{
    /*[SerializeField] private 卡牌 cardPrefab;
    [SerializeField] private Transform spawn;
    [SerializeField] private Transform hand;*/
    [SerializeField] private 手牌视图 handView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;//弃牌堆位置


    private readonly List<卡牌> drawPile = new();//存放抽牌堆所有卡片
    private readonly List<卡牌> discardPile = new();//弃牌堆
    private readonly List<卡牌> hand = new();//手牌
    private void OnEnable()
    {
        动作系统.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);   
        动作系统.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
        动作系统.AttachPerformer<PlayCardGA>(PlayCardPerformer);
        动作系统.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        动作系统.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        动作系统.DetachPerformer<DrawCardsGA>();
        动作系统.DetachPerformer<DiscardAllCardsGA>();
        动作系统.DetachPerformer<PlayCardGA>();
        动作系统.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        动作系统.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    public void Setup(List<卡牌数据> deckData)
    {
        foreach (var cardData in deckData)
        {
            卡牌 card =new(cardData);
            drawPile.Add(card);
        }
    }

    
    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)//抽牌执行者
    {
        int actualAmount = Mathf.Min(drawCardsGA.Amount,drawPile.Count);//实际能抽出多少张牌
        int notDrawnAmount=drawCardsGA.Amount-actualAmount;//将抽牌堆抽完后还需要抽多少张牌
        for (int i = 0; i < actualAmount; i++) 
        {
            yield return DrawCard();
        }

        if(notDrawnAmount>0)
        {
            RefillDeck();//洗牌
            for (int i = 0;i < notDrawnAmount ;i++)
            {
                yield return DrawCard();
            }
        }
    }

    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)//弃牌执行者
    {
        foreach (var card in hand)
        {
            discardPile.Add(card);
            卡片视图 cardView=handView.RemoveCard(card);
            yield return DiscardCard(cardView);
        }
        hand.Clear();
    }

    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        hand.Remove(playCardGA.Card);
        卡片视图 cardView=handView.RemoveCard(playCardGA.Card);
        yield return DiscardCard(cardView);
        SpendManaGA spendManaGA = new(playCardGA.Card.Mana);
        动作系统.Instance.AddReaction(spendManaGA);
        if (playCardGA.Card.ManualTargetEffect != null)
        {
            PerformEffectGA performEffectGA=new(playCardGA.Card.ManualTargetEffect,new() { playCardGA.ManualTarget });
            动作系统.Instance.AddReaction(performEffectGA);
        }
        foreach (var effectWrapper in playCardGA.Card.OtherEffects)
        {
            if(effectWrapper != null) 
            {
                List<CombatantView> targets = effectWrapper.TargetMode.GetTargets();
                PerformEffectGA performEffectGA=new(effectWrapper.Effect,targets);
                动作系统.Instance.AddReaction(performEffectGA);
            }
            
        }
    }

    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)//敌人反应前的动作
    {
        DiscardAllCardsGA discardAllCardsGA = new();//弃牌
        动作系统.Instance.AddReaction(discardAllCardsGA);
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)//敌人反应后的动作
    {
        RefillManaGA refillManaGA = new();
        动作系统.Instance.AddReaction(refillManaGA);
        DrawCardsGA drawCardsGA = new(5);//抽牌
        动作系统.Instance.AddReaction(drawCardsGA);
    }

    
    private IEnumerator DrawCard()//抽牌
    {
        if (drawPile.Count == 0)yield break;
        卡牌 card = drawPile.Draw();
        hand.Add(card);
        卡片视图 cardView=卡片视图创建器.Instance.CreateCardView(card,drawPilePoint.position,drawPilePoint.rotation);
        yield return handView.AddCard(cardView);
    }
    
    private void RefillDeck()
    {
        drawPile.AddRange(discardPile);//抽牌堆随机加入弃牌堆的牌
        discardPile.Clear();
    }

    private IEnumerator DiscardCard(卡片视图 cardView)//弃牌
    {
        cardView.transform.DOScale(Vector3.zero, 0.15f);
        Tween tween =cardView.transform.DOMove(discardPilePoint.position,0.15f);//将卡片视图丢弃至弃牌堆
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }

}
