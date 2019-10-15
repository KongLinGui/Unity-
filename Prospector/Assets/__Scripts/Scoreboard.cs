using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scoreboard类管理向玩家展示的分数
/// </summary>
public class Scoreboard : SingletonBase<Scoreboard> {
    public GameObject prefabFloatingScore;
    [SerializeField]
    private int _score = 0;
    public string _scoreString;

    /// <summary>
    /// score属性页也可以设置_scoreString
    /// </summary>
    public int score {
        get
        {
            return (_score);
        }
        set
        {
            _score = value;
            _scoreString = Utils.AddCommasToNumber(_score);
        }
    }

    /// <summary>
    /// scoreString属性页也可以设置GUIText.text
    /// </summary>
    public string scoreString
    {
        get
        {
            return (_scoreString);
        }
        set
        {
            _scoreString = value;
            GetComponent<GUIText>().text = _scoreString;
        }
    }

    /// <summary>
    /// 当SendMessage调用时，将fs.score加到this.score上
    /// </summary>
    /// <param name="fs"></param>
	public void FSCallback(FloatingScore fs)
    {
        score += fs.score;
    }

    /// <summary>
    /// 实例化一个新的FloatingScore游戏对象并初始化。它返回一个FloatingScore创建的指针，
    /// 这样调用函数可以完成更多功能（如设置fontSizes等）
    /// </summary>
    /// <param name="amt"></param>
    /// <param name="pts"></param>
    /// <returns></returns>
    public FloatingScore CreateFloatingScore(int amt, List<Vector3> pts)
    {
        GameObject go = Instantiate(prefabFloatingScore) as GameObject;
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject;//设置fs为回调的当前对象
        fs.Init(pts);
        return (fs);
    }
}
