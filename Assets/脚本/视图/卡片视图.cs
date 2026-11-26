using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class 卡片视图 : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text mana;//能量
    [SerializeField] private SpriteRenderer imagerSR;
    [SerializeField] private GameObject wrapper;
    [SerializeField] private LayerMask dropLayer;//放置区域的图层
    public 卡牌 Card {  get; private set; }
    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;
    public void SetUp(卡牌 card)
    {
        Card = card;
        title.text = card.Title;
        description.text = card.Description;
        mana.text=card.Mana.ToString();
        imagerSR.sprite=card.Image;
    }

    private void OnMouseEnter()
    {
        if (!Interactions.Instance.PlayerCanHover()) return;
        wrapper.SetActive(false);
        Vector3 pos =new(transform.position.x,-2,0);
        卡牌视图悬停.Instance.Show(Card,pos);
    }

    private void OnMouseExit()
    {
        if (!Interactions.Instance.PlayerCanHover()) return;
        卡牌视图悬停.Instance.Hide();
        wrapper.SetActive(true);
    }

    void OnMouseDown()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (Card.ManualTargetEffect != null)
        {
            ManualTargetSystem.Instance.StartTargeting(transform.position);
        }
        else
        {
            Interactions.Instance.PlayerIsDragging = true;
            wrapper.SetActive(true);
            卡牌视图悬停.Instance.Hide();
            dragStartPosition = transform.position;
            dragStartRotation=transform.rotation;
            transform.rotation=Quaternion.Euler(0,0,0);
            transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);//跟随鼠标移动
        }
            

    }

    void OnMouseUp()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (Card.ManualTargetEffect != null)
        {
            EnemyView target = ManualTargetSystem.Instance.EndTargeting(MouseUtil.GetMousePositionInWorldSpace(-1));
            if (target != null && ManaSystem.Instance.HasEnoughMana(Card.Mana))
            {
                PlayCardGA playCardGA =new(Card,target);
                动作系统.Instance.Perform(playCardGA);
            }
        }
        else
        {
            float dragDistance = Vector3.Distance(transform.position, dragStartPosition);
            // 如果拖拽距离小于阈值（例如0.5f），视为点击，恢复原位并取消拖拽状态
            if (dragDistance < 3f)
            {
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;
                Interactions.Instance.PlayerIsDragging = false;
                return;
            }

            // 排除卡牌自身的层
            //int layerMask = ~(1 << gameObject.layer);
            Vector3 mouseWorldPos = MouseUtil.GetMousePositionInWorldSpace(-1);
            if (Physics.Raycast(mouseWorldPos, Vector3.forward,out RaycastHit hit,10f, dropLayer) && ManaSystem.Instance.HasEnoughMana(Card.Mana))//击中某个区域且能量足够
            {
                //将牌打出
                PlayCardGA playCardGA = new(Card);
                动作系统.Instance.Perform(playCardGA);
            }
            else//恢复到初始位置
            {
                transform.position=dragStartPosition;
                transform.rotation=dragStartRotation;
            }
            Interactions.Instance.PlayerIsDragging = false;
        }
            
    }

    void OnMouseDrag()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if(Card.ManualTargetEffect!=null) return;
        transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
    }
}
