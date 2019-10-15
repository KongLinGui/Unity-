using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}

public class Layout : MonoBehaviour {
    public PT_XMLReader xmlr;//与Deck类一样，本类中也有一个PT_XMLReader
    public PT_XMLHashtable xml;//定义本变量是为了便于访问xml
    public Vector2 multiplier;//设置场景中牌的距离
    //SlotDef引用
    public List<SlotDef> slotDefs;//该List存储了从第0排到第3排中所有纸牌的slotDefs
    public SlotDef drawPile;
    public SlotDef discardPile;
    //以下字符串数组存储了根据LayerID确定的所有图层名称
    public string[] sortingLayerName = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

    //以下函数将被调用来读取LayoutXML.xml文件内容
	public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);//对XML格式字符串进行解析
        xml = xmlr.xml["xml"][0];//将xml设置为访问XML内容的快捷方式

        //读取用于设置纸牌间距的系数
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));
        //读入牌的位置
        SlotDef tSD;
        //slotsX是读取所有的<slot>的快捷方式
        PT_XMLHashList slotsX = xml["slot"];

        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDef();//新建一个SlotDef实例
            if (slotsX[i].HasAtt("type"))
            {
                //如果<slot>标签中有type属性，则解析其内容
                tSD.type = slotsX[i].att("type");
            }
            else
            {
                //如果没有type属性，则将type设置为"slot",表示场景中的纸牌
                tSD.type = "slot";
            }
            //各种属性均被解析为数值
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            tSD.layerName = sortingLayerName[tSD.layerID];
            switch (tSD.type)
            {
                case "slot":
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"));
                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (string s in hiding)
                        {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }
                    slotDefs.Add(tSD);
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
            }
        }
    }
}
