using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//用于处理所有可能的得分事件的枚举
public enum ScoreEvent
{
    draw,
    mine,
    mineGole,
    gameWin,
    gameLoss
}

public class Prospector : MonoBehaviour {
    static public Prospector S;
    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;

    public float reloadDelay = 1f;

    public Vector3 fsPosMid = new Vector3(0.5f, 0.9f, 0);
    public Vector3 fsPosRun = new Vector3(0.5f, 0.75f, 0);
    public Vector3 fsPosMid2 = new Vector3(0.5f, 0.5f, 0);
    public Vector3 fsPosEnd = new Vector3(1, 0.65f, 0);

    public Deck deck;
    public TextAsset deckXML;

    public Layout layout;
    public TextAsset layoutXML;
    public Vector3 layoutCenter;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Transform layoutAnchor;

    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;

    //记录得分信息的变量
    public int chain = 0;//当前回合的纸牌
    public int scoreRun = 0;
    public int score = 0;
    public FloatingScore fsRun;

    public GUIText GTGameOver;
    public GUIText GTRoundResult;
    void Awake()
    {
        S = this;
        //确认PlayerPrefs中的高分值
        if (PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }
        //将分数添加到上一轮，如果赢的话分数>0
        score += SCORE_FROM_PREV_ROUND;
        //并且重置SCORE_FROM_PREV_ROUND
        SCORE_FROM_PREV_ROUND = 0;

        //设置最后一轮显示的GUITexts
        //获取GUIText组件
        GameObject go = GameObject.Find("GameOver");
        if (go!=null)
        {
            GTGameOver = go.GetComponent<GUIText>();
        }
        go = GameObject.Find("RoundResult");
        if (go!=null)
        {
            GTRoundResult = go.GetComponent<GUIText>();
        }
        //使之不可见
        ShowResultGTs(false);

        go = GameObject.Find("HighScore");
        string hScore = "High Score:" + Utils.AddCommasToNumber(HIGH_SCORE);
        go.GetComponent<GUIText>().text = hScore;
    }

    void ShowResultGTs(bool show)
    {
        GTGameOver.gameObject.SetActive(show);
        GTRoundResult.gameObject.SetActive(show);
    }

    public List<CardProspector> drawPile;

	// Use this for initialization
	void Start () {
        Scoreboard.Instance.score = score;

        deck = GetComponent<Deck>();//获取Deck脚本组件
        deck.InitDeck(deckXML.text);//将deckXML传递给Deck脚本

        deck.Shuffle(ref deck.cards);//本行代码执行洗牌任务
        //ref关键字向deck.cards传递一个引用，        
        //使deck.Shuffle()可以对deck.cards进行操作

        layout = GetComponent<Layout>();//获取Layout脚本组件
        layout.ReadLayout(layoutXML.text);//将layoutXML传递给Layout脚本
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);

        LayoutGame();
    }

    //Draw将从drawPile取出一张纸牌并返回
    CardProspector Draw()
    {
        CardProspector cd = drawPile[0];//取出0号CardProspector
        drawPile.RemoveAt(0);//然后从List<>drawPile删除它
        return (cd);//最后返回它
    }

    //将整型layoutID转换为具有该ID的CardProspector
    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (CardProspector tCP in tableau)
        {
            //遍历tableau List<>中所有纸牌
            if (tCP.layoutID==layoutID)
            {
                //如果纸牌具有相同ID，则返回它
                return (tCP);
            }
        }
        //如果没找到，返回null
        return (null);
    }

    //定位纸牌的初始画面，即“矿井”
    void LayoutGame()
    {
        //创建一个空的游戏对象作为场景//1的锚点
        if (layoutAnchor==null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            //在层级结构中创建一个空的名为_LayoutAnchor的游戏对象
            layoutAnchor = tGO.transform;//获取Transform
            layoutAnchor.transform.position = layoutCenter;//定位
        }

        CardProspector cp;
        //按照布局
        foreach (SlotDef tSD in layout.slotDefs)
        {
            //遍历layout.slotDefs中为tSD的所有slotDefs
            cp = Draw();//从drawPile的顶部（开始）取一张纸牌
            cp.faceUp = tSD.faceUp;//设置该张纸牌的faceUp为SlotDef中的值
            cp.transform.parent = layoutAnchor;//设置它的父元素为layoutAnchor
            //替代先前的父元素deck.deckAnchor，即场景播放时出现在层级结构中的_Deck
            cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y,
                -tSD.layerID);
            //根据slotDef设置纸牌的localPosition
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = CardState.tableau;
            //画面中的CardProspectors具有CardState.tableau状态
            cp.SetSortingLayerName(tSD.layerName);//设置排序层
            tableau.Add(cp);
        }

        //设置纸牌间如何覆盖隐藏
        foreach (CardProspector tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        //设置初始目标纸牌
        MoveToTarget(Draw());
        //设置储备牌
        UpdateDrawPile();
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardProspector;//1
            lCP.Add(tCP);
        }
        return (lCP);
    }

    //游戏中任何时刻单击纸牌都会调用CardClicked
    public void CardClicked(CardProspector cd)
    {
        //根据被单击纸牌的状态进行响应
        switch (cd.state)
        {
            case CardState.drawpile:
                //单击任何储牌将抽出下一张牌   
                MoveToDiscard(target);//移动目标牌到弃牌堆
                MoveToTarget(Draw());//将抽出的牌移动为目标牌
                UpdateDrawPile();//重洗储备牌
                ScoreManager(ScoreEvent.draw);
                break;
            case CardState.tableau:
                //单击画面中的纸牌将检查是否为有效
                bool validMatch = true;
                if (!cd.faceUp)
                {
                    //如果纸牌朝下，则无效
                    validMatch = false;
                }
                if (!AdjacentRank(cd,target))
                {
                    //如果不为相邻点数，则无效
                    validMatch = false;
                }
                if (!validMatch) return;//无效则返回
                //这是一张有效牌
                tableau.Remove(cd);//从tableau List移除
                MoveToTarget(cd);//使之成为目标牌
                SetTableauFaces();//更新朝上纸牌
                ScoreManager(ScoreEvent.mine);
                break;
            case CardState.target:
                //单击目标纸牌无响应
                break;
            case CardState.discard:
                break;
        }
        //检查游戏是否结束
        CheckForGameOver();
    }

    //移动当前目标纸牌到弃牌堆
    void MoveToDiscard(CardProspector cd)
    {
        //设置纸牌的状态为丢弃
        cd.state = CardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;//更新transform父元素
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);
        //定位到弃牌堆
        cd.faceUp = true;
        //放到牌堆顶部用于深度排序
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    //使cd成为新的目标牌
    void MoveToTarget(CardProspector cd)
    {
        //如果当前已有目标牌，则将它移动到弃牌堆
        if (target!=null)
        {
            MoveToDiscard(target);
        }
        target = cd;//cd成为新的目标牌
        cd.state = CardState.target;
        cd.transform.parent = layoutAnchor;
        //移动到目标位置
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID);
        cd.faceUp = true;//纸牌正面朝上
        //设置深度排序
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    //排开所有储备牌显示剩余张数
    void UpdateDrawPile()
    {
        CardProspector cd;
        //遍历所有储备牌
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            //使用layout.drawPile.stagger精确定位
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i);
            cd.faceUp = false;//使所有牌朝下
            cd.state = CardState.drawpile;
            //设置深度排序
            cd.SetSortingLayerName(layout.discardPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    //如果2张牌为相邻点数则返回true(包括A&K)
    public bool AdjacentRank(CardProspector c0,CardProspector c1)
    {
        //如果有纸牌朝下，则不相邻
        if (!c0.faceUp||!c1.faceUp)
        {
            return (false);
        }
        //如果只差1个点数，则相邻
        if (Mathf.Abs(c0.rank-c1.rank)==1)
        {
            return (true);
        }
        //如果一个为A一个为K，则相邻
        if (c0.rank==1&&c1.rank==13)
        {
            return (true);
        }
        if (c0.rank==13&&c1.rank==1)
        {
            return (true);
        }
        //否则返回false
        return (false);
    }

    //纸牌变为朝上或朝下
    void SetTableauFaces()
    {
        foreach (CardProspector cd in tableau)
        {
            bool fup = true;//假设纸牌将朝上
            foreach (CardProspector cover in cd.hiddenBy)
            {
                //如果画面中有被盖住的纸牌
                if (cover.state==CardState.tableau)
                {
                    fup = false;//纸牌朝下
                }
            }
            cd.faceUp = fup;//设置纸牌分数
        }
    }

    //检查游戏是否结束
    void CheckForGameOver()
    {
        //如果画面为空，则游戏结束
        if (tableau.Count==0)
        {
            //调用GameOver()并且结果为赢
            GameOver(true);
            return;
        }
        //如果储备堆中仍有牌，则游戏未结束
        if (drawPile.Count>0)
        {
            return;
        }
        //检查剩余有效可玩纸牌
        foreach (CardProspector cd in tableau)
        {
            if (AdjacentRank(cd,target))
            {
                //如果有可玩纸牌，则游戏未结束
                return;
            }
        }
        //没有可玩纸牌，则游戏结束
        //调用GameOver并且结果为输
        GameOver(false);
    }

    //游戏结束时调用
    void GameOver(bool won)
    {
        if (won)
        {
            //print("Game Over.You won! :)");
            ScoreManager(ScoreEvent.gameWin);
        }
        else
        {
            //print("Game Over.You Lost. :(");
            ScoreManager(ScoreEvent.gameLoss);
        }
        //在reloadDelay时间内重新加载场景
        //定义分数环绕屏幕的时刻
        Invoke("ReloadLevel", reloadDelay);
    }

    void ReloadLevel()
    {
        //重新加载场景，重置游戏
        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    //ScoreManager处理所有得分
    void ScoreManager(ScoreEvent sEvt)
    {
        List<Vector3> fsPts;
        switch (sEvt)
        {
            //无论是抽牌、赢或输，需要有对应的动作
            case ScoreEvent.draw://抽一张牌           
            case ScoreEvent.gameWin://赢得本轮
            case ScoreEvent.gameLoss://本轮输了
                chain = 0;//重置分数变量chain
                score += scoreRun;//将scoreRun加入总得分
                scoreRun = 0;//重置scoreRun
                //将fsRun添加到_Scoreboard分数
                if (fsRun!=null)
                {
                    //创建贝塞尔曲线的坐标点
                    fsPts = new List<Vector3>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.Instance.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    //同时调整fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null;//清除fsRun以再次创建
                }
                break;
            case ScoreEvent.mine://删除一张矿井纸牌
                chain++;//分数变量chain自加
                scoreRun += chain;//添加当前纸牌的分数到这回合
                //为当前分数创建FloatingScore
                FloatingScore fs;
                //从mousePosition移动到fsPosRun
                Vector3 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector3>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.Instance.CreateFloatingScore(chain, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun==null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
        }

        //第二个switch语句处理本轮的输赢
        switch (sEvt)
        {
            case ScoreEvent.gameWin:
                GTGameOver.text = "Round Over";
                //赢的话，将分数添加到下一轮
                Prospector.SCORE_FROM_PREV_ROUND = score;
                GTRoundResult.text = "You won this round!\nRound Score:" + score;
                ShowResultGTs(true);
                break;
            case ScoreEvent.gameLoss:
                GTGameOver.text = "Game Over";
                //输的话，与最高分进行比较
                if (Prospector.HIGH_SCORE<=score)
                {
                    string sRR = "You got the hihg score!\nHigh score:" + score;
                    GTRoundResult.text = sRR;
                    Prospector.HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else
                {
                    GTRoundResult.text = "Your final score was:" + score;
                }
                ShowResultGTs(true);
                break;
            default:
                break;
        }
    }
}
