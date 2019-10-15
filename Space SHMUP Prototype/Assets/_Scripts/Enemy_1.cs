using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_1 : Enemy {
    //完成一个完整的正弦曲线周期所需的时间
    public float waveFrequency = 2;
    //正弦曲线的宽度，以米为单位
    public float waveWidth = 4;
    public float waveRotY = 45;
    private float x0 = -12345;//初始位置的x坐标
    private float birthTime;

	void Start () {
        x0 = pos.x;
        birthTime = Time.time;
	}

    //重写Enemy的Move函数
    public override void Move()
    {
        //因为pos是一种属性，不能直接设置pos.x
        //所以将pos赋给一个可以修改的三维向量变量
        Vector3 tempPos = pos;
        //基于时间调整theta值
        float age = Time.time - birthTime;
        float theta = Mathf.PI * 2 * age / waveFrequency;
        float sin = Mathf.Sin(theta);
        tempPos.x = x0 + waveWidth * sin;
        pos = tempPos;
        //让对象绕y 轴稍作旋转
        Vector3 rot = new Vector3(0, sin * waveRotY, 0);
        this.transform.rotation = Quaternion.Euler(rot);
        //对象在y方向上的运动仍有base.Move()函数处理
        base.Move();
    }
}
