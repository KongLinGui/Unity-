using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLine : MonoBehaviour {
    static public ProjectileLine S;
    public float minDist = 0.1f;
    public LineRenderer line;
    private GameObject _poi;
    public List<Vector3> points;
	
	void Awake () {
        S = this;
        line = GetComponent<LineRenderer>();
        line.enabled = false;
        //初始化三维向量点的List
        points = new List<Vector3>();
	}
	
    public GameObject poi
    {
        get
        {
            return (_poi);
        }
        set
        {
            _poi = value;
            if (_poi!=null)
            {
                //当把_poi设置为新对象时，将复位其所有内容
                line.enabled = false;
                points = new List<Vector3>();
                AddPoint();
            }
        }
    }
    /// <summary>
    /// 这个函数用于直接清除线条
    /// </summary>
    public void Clear()
    {
        _poi = null;
        line.enabled = false;
        points = new List<Vector3>();
    }

    public void AddPoint()
    {
        //用于在线条上添加一个点
        Vector3 pt = _poi.transform.position;
        if (points.Count>0&&(pt-lastPoint).magnitude<minDist)
        {//如果该点与上一个点的位置不够远，则返回
            return;
        }
        if (points.Count==0)
        {
            //如果当前是发射点
            Vector3 launchPos = Slingshot.S.launchPoint.transform.position;
            Vector3 launchPosDiff = pt - launchPos;
            //则添加一根线条，帮助之后瞄准
            points.Add(pt + launchPosDiff);
            points.Add(pt);
            line.SetVertexCount(2);
            //设置前两个点
            line.SetPosition(0, points[0]);
            line.SetPosition(1, points[1]);
            //启用线渲染器
            line.enabled = true;
        }
        else
        {
            //正常添加点的操作
            points.Add(pt);
            line.SetVertexCount(points.Count);
            line.SetPosition(points.Count - 1, lastPoint);
            line.enabled = true;
        }
    }

    /// <summary>
    /// 返回最近添加的点的位置
    /// </summary>
    public Vector3 lastPoint
    {
        get
        {
            if (points==null)
            {
                //如果当前还没有点，返回Vector3.zero
                return (Vector3.zero);
            }
            return (points[points.Count - 1]);
        }
    }

	void FixedUpdate () {
        if (poi==null)
        {
            //如果兴趣点不存在，则找出下一个
            if (FollowCam.S.poi!=null)
            {
                if (FollowCam.S.poi.tag == "Projectile")
                {
                    poi = FollowCam.S.poi;
                }
                else
                {
                    return;//如果未找到兴趣点，则返回
                }
            }
            else
            {
                return;//如果未找到兴趣点，则返回
            }
        }
        //如果存在兴趣点，则在FixedUpdate中在其位置上增加一个点
        AddPoint();
        if (poi.GetComponent<Rigidbody>().IsSleeping())
        {
            //当兴趣点静止时，将其清空（设置为null）
            poi = null;
        }
	}
}
