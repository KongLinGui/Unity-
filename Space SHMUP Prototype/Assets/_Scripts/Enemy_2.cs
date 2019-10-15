using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_2 : Enemy {
    //Enemy_2使用正弦波形修正两点插值
    public Vector3[] points;
    public float birthTime;
    public float lifeTime=10;
    //确定正弦波形对运动的影响程度
    public float sinEccentricity = 0.6f;

    void Start () {
        //初始化节点数组
        points = new Vector3[2];
        //找到摄像机边界框Utils.camBounds
        Vector3 cbMin = Utils.camBounds.min;
        Vector3 cbMax = Utils.camBounds.max;
        Vector3 v = Vector3.zero;
        //从屏幕左侧随意选取一点
        v.x = cbMin.x - Main.S.enemySpawnPadding;
        v.y = Random.Range(cbMin.y, cbMax.y);
        points[0] = v;
        //从屏幕右侧随意选取一点
        v = Vector3.zero;
        v.x = cbMax.x + Main.S.enemySpawnPadding;
        v.y = Random.Range(cbMin.y, cbMax.y);
        points[1] = v;
        //有一半可能会换边
        if (Random.value<0.5f)
        {
            //将每个点的x值设为它的相反数
            //可以将这个点移动到屏幕的另一侧
            points[0].x *= -1;
            points[1].x *= -1;
        }
        //设置出生时间为当前时间
        birthTime = Time.time;
	}

    public override void Move()
    {
        //贝塞尔曲线的形成基于一个0到1之间的u值
        float u = (Time.time - birthTime) / lifeTime;

        //如果u>1,则表示自birthTime到当前的时间间隔已经大于生命周期lifeTime
        if (u>1)
        {
            //所以当前的Enemy_2实例将终结自己
            Destroy(this.gameObject);
            return;
        }
        //通过叠加一个基于正弦曲线的平滑曲线调整u值
        u = u + sinEccentricity * (Mathf.Sin(u * Mathf.PI * 2));

        //在两点之间进行插值
        pos = (1 - u) * points[0] + u * points[1];
    }
}
