using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Decorator
{
    //此类用于存储来自deckXML的角码符号（包括纸牌角部的点数和花色符号）的信息
    public string type;//对于花色符号，type="pip"
    public Vector3 loc;//spite在纸片上的位置信息
    public bool flip = false;//是否垂直翻转spirte
    public float scale = 1f;//spirte的缩放比例
}

[System.Serializable]
public class CardDefinition
{
    //此类用于存储各点数的牌面信息
    public string face;//各张花牌（J、Q、K）所用的spirte名称
    public int rank;//本张牌的点数（1-13）
    public List<Decorator> pips = new List<Decorator>();//用到的花色符号
    //因为每张牌面上角码（读取自XML文件）的布局都相同
    //所以pips列表中只存储数字牌上花色符号的信息
}
public class Card : MonoBehaviour {
    public string suit;//牌的花色（红桃、黑桃、方片或梅花）
    public int rank;//牌的点数（1-13）
    public Color color = Color.black;//花色符号的颜色
    public string colS = "Black";//颜色的名称，值为“Black”或“Red”
    //以下List存储所有的Decorator游戏对象
    public List<GameObject> decoGOs = new List<GameObject>();
    //以下List存储所有的Pip游戏对象
    public List<GameObject> pipGOs = new List<GameObject>();
    public GameObject back;//纸牌背面图像的游戏对象
    public CardDefinition def;//该变量的值解析自DeckXML.xml

    //当前游戏对象的组件列表及其子类
    public SpriteRenderer[] spriteRenderers;

    void Start()
    {
        SetSortOrder(0);//保证纸牌开始于正确的深度排序
    }

    public bool faceUp
    {
        get
        {
            return (!back.activeSelf);
        }
        set
        {
            back.SetActive(!value);
        }
    }

    //如果未定义spriteRenderers，使用该函数定义
    public void PopulateSpriteRenderers()
    {
        //如果spriteRenderers为null或empty
        if (spriteRenderers==null||spriteRenderers.Length==0)
        {
            //获取当前游戏对象的SpriteRenderer组件及其子类
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    //设置所有SpriteRenderer组件的sortingLayerName
    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }

    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();
        //遍历所有为tSR的spriteRenderers
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            if (tSR.gameObject==this.gameObject)
            {
                //如果gameObject为this.gameObject，则为背景
                tSR.sortingOrder = sOrd;//设置顺序为sOrd
                continue;//继续遍历下一个循环
            }
            //GameObject的每一个子对象都根据name变换名称
            switch (tSR.gameObject.name)
            {
                case "back":
                    tSR.sortingOrder = sOrd + 2;
                    //设置为最高层覆盖所有
                    break;
                case "face":
                default:
                    tSR.sortingOrder = sOrd + 1;
                    //设置为中层置于背景之上
                    break;
            }
        }
    }

    //通过在子类函数中使用相同名字可以重写虚函数
    virtual public void OnMouseUpAsButton()
    {
        print(name);//单击时，输出纸牌名
    }
}
