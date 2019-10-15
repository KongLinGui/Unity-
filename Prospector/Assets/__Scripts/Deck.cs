using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour {
    //花色
    public Sprite suitClub;//梅花
    public Sprite suitDiamond;//方片
    public Sprite suitHeart;//红桃
    public Sprite suitSpade;//黑桃
    public Sprite[] faceSprites;//花牌
    public Sprite[] rankSprites;//点数
    public Sprite cardBack;//普通绝牌背面
    public Sprite cardBackGold;//金色纸牌背面
    public Sprite cardFront;//普通纸牌正面的背景
    public Sprite cardFrontGold;//金色纸牌正面的背景
    //预设
    public GameObject prefabSprite;
    public GameObject prefabCard;
    public bool _______________;
    public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    //当Prospector脚本运行时，将调用这里的InitDeck函数
    public void InitDeck(string deckXMLText)
    {
        //以下语句为层级面板中的所有Card游戏对象创建一个锚点
        if (GameObject.Find("_Deck")==null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }
        //使用所有必须的Sprite初始化SuitSprites字典
        dictSuits = new Dictionary<string, Sprite>()
        {
            {"C",suitClub },
            {"D",suitDiamond },
            {"H",suitHeart },
            {"S",suitSpade }
        };
        ReadDeck(deckXMLText);
        MakeCards();
    }

    //ReadDeck函数将传入的XML文件解析为CardDefinition类的实例
    public void ReadDeck(string deckXMLText)
    {
        xmlr = new PT_XMLReader();//新建一个XML读取器PT_XMLReader
        xmlr.Parse(deckXMLText);//使用这个PT_XMLReader解析DeckXML文件
        //这里将输出一条测试语句，演示xmlr如何使用
        string s = "xml[0] decorator[0]";
        s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");
        //print(s);

        //读取所有纸牌的角码(Decorator)
        decorators = new List<Decorator>();//初始化一个Decorator对象列表
        //从XML文件中获取所有<decorator>标签，构成一个PT_XMLHashList列表
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;
        for (int i = 0; i < xDecos.Count; i++)
        {
            //对于XML中的每一个<decorator>
            deco = new Decorator();//创建一个新的Decorator对象
            //将<decorator>标签中的所有属性复制给该Decorator对象
            deco.type = xDecos[i].att("type");
            //根据<decorator>标签的flip的属性是否为1，设置Decorator对象
            deco.flip = (xDecos[i].att("flip") == "1");
            //浮点数需要从属性字符串中解析出来
            deco.scale = float.Parse(xDecos[i].att("scale"));
            //三维向量loc已初始化为[0,0,0],我们只需要修改其值
            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));
            //将临时变量deco添加到有角码构成的List
            decorators.Add(deco);
        }

        //读取每种点数对应的花色符号位置
        cardDefs = new List<CardDefinition>();//初始化由CardDefinition构成的List
        //从XML文件中获取所有<card>标签，构成一个PT_XMLHashList列表
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
        for (int i = 0; i < xCardDefs.Count; i++)
        {
            //对于每个<card>标签
            //创建一个新的CardDefinition变量cDef
            CardDefinition cDef = new CardDefinition();
            //解析其属性值并添加到cDef中
            cDef.rank = int.Parse(xCardDefs[i].att("rank"));
            //获取当前<card>标签中所有的<pip>标签，构成一个PT_XMLHashList列表
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips!=null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    //遍历所有的<pip>标签
                    deco = new Decorator();
                    //通过Decorator类处理<card>中的<pip>标签
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));
                    if (xPips[j].HasAtt("scale"))
                    {
                        deco.scale = float.Parse(xPips[j].att("scale"));
                    }
                    cDef.pips.Add(deco);
                }
            }
            //花牌(J,Q,K)包含一个face属性
            //cDef.face是花牌Sprite的基本名称
            //例如，J的基本名称时FaceCard_11
            //而梅花J的名称时FaceCard_11C，红桃J的名称是FaceCard_11H等
            if (xCardDefs[i].HasAtt("face"))
            {
                cDef.face = xCardDefs[i].att("face");
            }
            cardDefs.Add(cDef);
        }
    }

    //根据点数（1-13分别代表绝牌的A-K）获取对应的CardDefinition（牌面布局定义）
    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        //搜索所有的CardDefinition
        foreach (CardDefinition cd in cardDefs)
        {
            //如果点数正确，返回相应的定义
            if (cd.rank==rnk)
            {
                return (cd);
            }
        }
        return (null);
    }

    //创建Card游戏对象
    public void MakeCards()
    {
        //List型变量cardNames中是要创建的纸牌的名称
        //每种花色均包含1到13的点数（例如黑桃为C1到C13）
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }
        //创建一个List,用于存储所有的纸牌
        cards = new List<Card>();
        //有些变量会重复使用多次
        Sprite tS = null;
        GameObject tGO = null;
        SpriteRenderer tSR = null;
        //遍历前面得到的所有纸牌名称
        for (int i = 0; i < cardNames.Count; i++)
        {
            //创建一个新的Card游戏对象
            GameObject cgo = Instantiate(prefabCard) as GameObject;
            //将transform.parent设置为锚点
            cgo.transform.parent = deckAnchor;
            Card card = cgo.GetComponent<Card>();//获取Card组件
            //以下语句用于排列纸牌，使其整齐摆放
            cgo.transform.localPosition = new Vector3((i % 13) * 3, i / 13 * 4, 0);
            //设置纸牌的基本属性值
            card.name = cardNames[i];
            card.suit = card.name[0].ToString();
            card.rank = int.Parse(card.name.Substring(1));
            if (card.suit=="D"||card.suit=="H")
            {
                card.colS = "Red";
                card.color = Color.red;
            }
            //提取本张纸牌的定义
            card.def = GetCardDefinitionByRank(card.rank);
            //添加角码
            foreach (Decorator deco in decorators)
            {
                if (deco.type=="suit")
                {
                    //初始化一个Sprite游戏对象
                    tGO = Instantiate(prefabSprite) as GameObject;
                    //获取SpriteRenderer组件
                    tSR = tGO.GetComponent<SpriteRenderer>();
                    //将Sprite设置为正确的花色
                    tSR.sprite = dictSuits[card.suit];
                }
                else
                {
                    //如果不是花色符号，那就是点数
                    tGO = Instantiate(prefabSprite) as GameObject;
                    tSR = tGO.GetComponent<SpriteRenderer>();
                    //获取正确的Sprite来显示该点数
                    tS = rankSprites[card.rank];
                    //将表示点数的Sprite赋给SpriteRenderer
                    tSR.sprite = tS;
                    //使点数符号的颜色与纸牌的花色相符
                    tSR.color = card.color;
                }
                //使表示角码的Sprite显示在纸牌之上
                tSR.sortingOrder = 1;
                //使表示角码的Sprite成为纸牌的子对象
                tGO.transform.parent = cgo.transform;
                //根据DeckXML中的位置设置localPosition
                tGO.transform.localPosition = deco.loc;
                //如果有必要，则翻转角码
                if (deco.flip)
                {
                    //让角码沿z轴进行180的欧拉旋转，即会使它翻转
                    tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                //设置角码的缩放比例，以免其尺寸过大
                if (deco.scale != 1)
                {
                    tGO.transform.localScale = Vector3.one * deco.scale;
                }
                //为游戏对象指定名称，使其易于查找
                tGO.name = deco.type;
                //将这个deco游戏对象添加到card.decoGOs列表List中
                card.decoGOs.Add(tGO);
            }
            //添加中间的花色符号
            //对于定义内容中的每个花色符号
            foreach (Decorator pip in card.def.pips)
            {
                //初始化一个Sprite游戏对象
                tGO = Instantiate(prefabSprite) as GameObject;
                //将Card设置为它的父对象
                tGO.transform.parent = cgo.transform;
                //按照XML内容设置其位置
                tGO.transform.localPosition = pip.loc;
                //必要时进行翻转
                if (pip.flip)
                {
                    tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                //必要时进行缩放（只适用于点数为A的情况）
                if (pip.scale!=1)
                {
                    tGO.transform.localScale = Vector3.one * pip.scale;
                }
                //为游戏对象指定名称
                tGO.name = "pip";
                //获取它的SpriteRenderer组件
                tSR = tGO.GetComponent<SpriteRenderer>();
                //将Sprite设置为正确的花色符号
                tSR.sprite = dictSuits[card.suit];
                //设置sortingOrder，使花色符号显示在纸牌背景Card_Front之上
                tSR.sortingOrder = 1;
                //
                card.pipGOs.Add(tGO);
            }

            //处理花牌(J、Q、K)
            if (card.def.face!="")
            {
                //如果card.def的face字段不为空（表示纸牌有牌面图案）
                tGO = Instantiate(prefabSprite) as GameObject;
                tSR = tGO.GetComponent<SpriteRenderer>();
                //生成正确的名称并传递给GetFace()
                tS = GetFace(card.def.face + card.suit);
                tSR.sprite = tS;//将这个Sprite赋给tSR变量
                tSR.sortingOrder = 1;//设置sortingOrder
                tGO.transform.parent = card.transform;
                tGO.transform.localPosition = Vector3.zero;
                tGO.name = "face";
            }

            //添加纸牌背景
            //Card_Back将覆盖纸牌上的所有其他元素
            tGO = Instantiate(prefabSprite) as GameObject;
            tSR = tGO.GetComponent<SpriteRenderer>();
            tSR.sprite = cardBack;
            tGO.transform.parent = card.transform;
            tGO.transform.localPosition = Vector3.zero;
            //它的sortingOrder值高于纸牌上的所有其他元素
            tSR.sortingOrder = 2;
            tGO.name = "back";
            card.back = tGO;
            //faceUp的默认值
            card.faceUp = false;//使用Card的faceUp属性
            //将这张纸牌添加到整幅牌中
            cards.Add(card);
        }
    }

    //查找正确的花牌Sprite
    public Sprite GetFace(string faceS)
    {
        foreach (Sprite tS in faceSprites)
        {
            //如果Sprite名称正确
            if (tS.name==faceS)
            {
                //则返回这个Sprite
                return (tS);
            }
        }
        //如果查找不到，则返回null
        return (null);
    }

    //为Deck.cards中的纸牌洗牌
    public void Shuffle(ref List<Card> oCards)
    {
        //创建一个临时List,用于存储洗牌后纸牌的新顺序
        List<Card> tCards = new List<Card>();
        int ndx;//这个变量将存储要移动的纸牌的索引
        tCards = new List<Card>();//初始化临时List

        //只要原始List中还有纸牌，就一直循环
        while (oCards.Count>0)
        {
            //随机抽取一张牌，并得到它的索引
            ndx = Random.Range(0, oCards.Count);
            //把这张纸牌加入到临时List中
            tCards.Add(oCards[ndx]);
            //同时把它从原始List中删除
            oCards.RemoveAt(ndx);
        }
        //用新的临时List取代原始List
        oCards = tCards;
        //因为oCards是一个引用型变量，所以传入的原始List也会被修改
    }
}
