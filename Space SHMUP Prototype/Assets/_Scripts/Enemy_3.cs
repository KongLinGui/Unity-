using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_3 : Enemy {
    //Enemy_3将沿贝塞尔曲线运动
    //贝塞尔曲线由两点之间的线性插值点构成
    public Vector3[] points;
    public float birthTime;
    public float lifeTime = 10;

    void Start () {
        points = new Vector3[3];//初始化节点数组
        //初始位置已经在中进行过设置
        points[0] = pos;
        //xMin和xMax值的设置方法与Main.SpawnEnemy()中相同
        float xMin = Utils.camBounds.min.x + Main.S.enemySpawnPadding;
        float xMax = Utils.camBounds.max.x - Main.S.enemySpawnPadding;
        Vector3 v;
        //在屏幕下部随机选取一个点作为中间节点
        v = Vector3.zero;
        v.x = Random.Range(xMin, xMax);
        v.y = Random.Range(Utils.camBounds.min.y, 0);
        points[1] = v;
        //在屏幕上部随机选取一个点作为终点
        v = Vector3.zero;
        v.y = pos.y;
        v.x = Random.Range(xMin, xMax);
        points[2] = v;
        //设置出生时间birthTime为当前时间
        birthTime = Time.time;
	}

    public override void Move()
    {
        //贝塞尔曲线的形成基于一个0到1之间的u值
        float u = (Time.time - birthTime) / lifeTime;
        if (u > 1)
        {
            //所以当前的Enemy_3实例将终结自己
            Destroy(this.gameObject);
            return;
        }
        //在三点贝塞尔曲线上插值
        Vector3 p01, p12;
        u = u - 0.2f * Mathf.Sin(u * Mathf.PI * 2);
        p01 = (1 - u) * points[0] + u * points[1];
        p12 = (1 - u) * points[1] + u * points[2];
        pos = (1 - u) * p01 + u * p12;
        base.Move();
    }
}
