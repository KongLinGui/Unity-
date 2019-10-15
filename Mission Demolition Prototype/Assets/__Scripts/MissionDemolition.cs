using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    idle,
    playing,
    levelEnd
}

public class MissionDemolition : MonoBehaviour {
    static public MissionDemolition S;
    public GameObject[] castles;//存储所有城堡对象的数组
    public GUIText gtLevel;//GT_Level界面文字
    public GUIText gtScore;//GT_Score界面文字
    public Vector3 castlePos;//放置城堡的位置
    public int level;//当前级别
    public int levelMax;//级别的数量
    public int shotsTaken;
    public GameObject castle;//当前城堡
    public GameMode mode = GameMode.idle;
    public string showing = "Slingshot";//摄像机的模式
	
	void Start () {
        S = this;
        level = 0;
        levelMax = castles.Length;
        StartLevel();
    }
	
    void StartLevel()
    {
        //如果已经有城堡存在，则清除原有的城堡
        if (castle!=null)
        {
            Destroy(castle);
        }
        //清除原有的弹丸
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Projectile");
        foreach (GameObject pTemp in gos)
        {
            Destroy(pTemp);
        }
        //实例化新城堡
        castle = Instantiate(castles[level] as GameObject);
        castle.transform.position = castlePos;
        shotsTaken = 0;
        //重置摄像机位置
        SwitchView("Both");
        ProjectileLine.S.Clear();
        //重置目标状态
        Goal.goalMet = false;
        ShowGT();
        mode = GameMode.playing;
    }
	
    void ShowGT()
    {
        //设置界面文字
        gtLevel.text = "Level:" + (level) + "of" + levelMax;
        gtScore.text = "Shots Taken:" + shotsTaken;
    }

	void Update () {
        ShowGT();
        //检查是否已完成该级别
        if (mode==GameMode.playing&&Goal.goalMet)
        {
            //当完成级别时，改变mode，停止检查
            mode = GameMode.levelEnd;
            //缩小画面比例
            SwitchView("Both");
            //在2秒后开始下一级别
            Invoke("NextLevel", 2f);
        }
	}

    void NextLevel()
    {
        level++;
        if (level==levelMax)
        {
            level = 0;
        }
        StartLevel();
    }

    void OnGUI()
    {
        //在屏幕顶端绘制用户界面按钮，用于切换视图
        Rect buttonRect = new Rect((Screen.width / 2) - 50, 10, 100, 24);
        switch (showing)
        {
            case "Slingshot":
                if (GUI.Button(buttonRect,"查看城堡"))
                {
                    SwitchView("Castle");
                }
                break;
            case "Castle":
                if (GUI.Button(buttonRect, "查看全部"))
                {
                    SwitchView("Both");
                }
                break;
            case "Both":
                if (GUI.Button(buttonRect, "查看弹弓"))
                {
                    SwitchView("Slingshot");
                }
                break;
        }
    }

    //允许在代码任意位置切换视图的静态方法
    static public void SwitchView(string eView)
    {
        S.showing = eView;
        switch (S.showing)
        {
            case "Slingshot":
                FollowCam.S.poi = null;
                break;
            case "Castle":
                FollowCam.S.poi = S.castle;
                break;
            case "Both":
                FollowCam.S.poi = GameObject.Find("ViewBoth");
                break;
        }
    }

    //允许在代码任意位置增加发射次数的代码
    public static void ShotFired()
    {
        S.shotsTaken++;
    }
}
