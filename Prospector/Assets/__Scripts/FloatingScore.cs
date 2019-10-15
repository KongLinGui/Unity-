using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//用于记录FloatingScore所有状态的枚举
public enum FSState
{
    idle,
    pre,
    active,
    post
}

/// <summary>
/// FloatingScore可以在屏幕上沿着贝塞尔曲线移动
/// </summary>
public class FloatingScore : MonoBehaviour {
    public FSState state = FSState.idle;
    [SerializeField]
    private int _score = 0;//分数变量
    public string scoreString;

    /// <summary>
    /// score属性页可设置scoreString
    /// </summary>
    public int score
    {
        get
        {
            return (_score);
        }
        set
        {
            _score = value;
            scoreString = Utils.AddCommasToNumber(_score);
            GetComponent<GUIText>().text = scoreString;
        }
    }

    public List<Vector3> bezierPts;//用于移动的Bezier坐标
    public List<float> fontSizes;//用于字体缩放的Bezier坐标
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCuve = Easing.InOut;//使用Utils.cs的Easing
    //移动完成时游戏对象将接收SendMessage
    public GameObject reportFinishTo = null;

    /// <summary>
    /// 设置FloatingScore和移动
    /// </summary>
    /// <param name="ePts"></param>
    /// <param name="eTimeS"></param>
    /// <param name="eTimeD"></param>
    public void Init(List<Vector3>ePts,float eTimeS=0,float eTimeD = 1)
    {
        bezierPts = new List<Vector3>(ePts);

        if (ePts.Count==1)//如果只有一个坐标
        {
            //只运行至此
            transform.position = ePts[0];
            return;
        }

        //如果eTimeS为缺省值，就从当前时间开始
        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;

        state = FSState.pre;//设置为pre state，准备好开始移动
    }

    public void FSCallback(FloatingScore fs)
    {
        //当SendMessage调用这个callback时，从参数FloatingScore获得要加的分数
        score += fs.score;
    }

	void Update () {
        //如果没有移动，则返回
        if (state == FSState.idle) return;

        //从当前时间和持续时间计算u，u范围为0到1（通常）
        float u = (Time.time - timeStart) / timeDuration;
        //使用Utils的Easing类描绘u值曲线图
        float uC = Easing.Ease(u, easingCuve);
        if (u<0)//如果u<0,那么还不能移动
        {
            state = FSState.pre;
            //移动到初始坐标
            transform.position = bezierPts[0];
        }
        else
        {
            if (u>=1)//如果u>=1，已完成移动
            {
                uC = 1;//设置uC = 1避免越界溢出
                state = FSState.post;
                if (reportFinishTo!=null)//如果有回调GameObject
                {
                    //使用SendMessage调用FSCallback方法，并带this参数
                    reportFinishTo.SendMessage("FSCallback", this);
                    //消息发送后，销毁当前gameObject
                    Destroy(gameObject);
                }
                else//如果没有回调
                {
                    //不销毁当前对象，仅保存
                    state = FSState.idle;
                }
            }
            else
            {
                //0<=u<1代表当前对象有效且正在移动
                state = FSState.active;
            }
            //使用Bezier曲线将当前对象移动到正确坐标
            Vector3 pos = Utils.Bezier(uC, bezierPts);
            transform.position = pos;
            if (fontSizes!=null&&fontSizes.Count>0)//如果fontSizes有值
            {
                //那么调整GUIText的fontSize
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<GUIText>().fontSize = size;
            }
        }
	}
}
