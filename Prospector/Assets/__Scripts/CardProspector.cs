using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//这个枚举定义的变量类型为只具有特定名称值
public enum CardState
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardProspector : Card
{//确保CardProspector从Card继承
    //枚举CardState的使用方式
    public CardState state = CardState.drawpile;
    //hiddenBy列表保存了当前纸牌朝下的其他纸牌
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    //layoutID对当前纸牌和Layout XML id进行匹配，判断是否为场景纸牌
    public int layoutID;
    //SlotDef存储从LayoutXML<slot>导入的信息
    public SlotDef slotDef;

    //使得纸牌可以响应单击动作
    public override void OnMouseUpAsButton()
    {
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}
