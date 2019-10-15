using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour {
    //在unity检视面板中设置的字段
    public GameObject prefabProjectile;
    public float velocityMult = 4f;
    
    //动态设置的字段
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;

    static public Slingshot S;

    void Awake()
    {
        S = this;
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;
    }

    void Update()
    {
        //如果弹弓未处于瞄准模式aimingMode，则跳过以下代码
        if (!aimingMode) return;
        //获取鼠标光标在二维窗口中的当前坐标
        Vector3 mousePos2D = Input.mousePosition;
        //将鼠标光标位置转换为三维世界坐标
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);
        //计算launchPos到mousePos3D两点之间的坐标差
        Vector3 mouseDelta = mousePos3D - launchPos;
        //将mouseDelta坐标差限制在弹弓的球状碰撞器半径范围内
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude>maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }
        //将projectile移动到新位置
        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;
        if (Input.GetMouseButtonUp(0))
        {
            //如果已经松开鼠标
            aimingMode = false;
            projectile.GetComponent<Rigidbody>().isKinematic = false;
            projectile.GetComponent<Rigidbody>().velocity = -mouseDelta * velocityMult;
            FollowCam.S.poi = projectile;
            projectile = null;
            MissionDemolition.ShotFired();
        }
    }

    void OnMouseEnter()
    {
        //print("Slingshot:OnMouseEnter()");
        launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        //print("Slingshot:OnMouseExit()");
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        //玩家在鼠标光标悬停在弹弓上方时按下了鼠标左键
        aimingMode = true;
        //实例化一个弹丸
        projectile = Instantiate(prefabProjectile) as GameObject;
        //该实例的初始位置位于launchPoint处
        projectile.transform.position = launchPos;
        //设置当前的属性
        projectile.GetComponent<Rigidbody>().isKinematic = true;
    }
}
