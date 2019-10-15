using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//这个枚举类型中定义了所有可能出现的武器类型
//其中也包含了可以用于升级护盾的“shield”类型
//标记[NI]的项目表示未实现
public enum WeaponType
{
    none,       //默认/无武器
    blaster,    //简单的高爆弹武器
    spread,     //同时发射两发炮弹
    phaser,     //沿波形前进[NI]
    missile,    //制导导弹[NI]
    laser,      //按时间造成伤害[NI]
    shield      //提高护盾等级
}

//WeaponDefinition类可以让你在检视面板中设置特定武器属性
//Main脚本中有一个WeaponDefinition的数组，可以在其中进行设置
[System.Serializable]
public class WeaponDefinition
{
    public WeaponType type = WeaponType.none;
    public string letter;//升级道具中显示的字母
    public Color color = Color.white;//Collar 升级道具的颜色
    public GameObject projectilePrefab;//炮弹预设
    public Color projectileColor = Color.white;
    public float damageOnHit = 0;//造成的伤害点数
    public float continuousDamage = 0;//每秒伤害点数(Laser)
    public float delayBetweenShots = 0;
    public float velocity = 20;//炮弹的速度
}

public class Weapon : MonoBehaviour {
    static public Transform PROJECTILE_ANCHOR;
    [SerializeField]
    private WeaponType _type = WeaponType.blaster;
    public WeaponDefinition def;
    public GameObject collar;
    public float lastShot;

    void Awake()
    {
        collar = transform.Find("Collar").gameObject;
    }

	void Start () {       
        //调用SetType()，正确设置默认武器类型_type
        SetType(_type);
        if (PROJECTILE_ANCHOR==null)
        {
            GameObject go = new GameObject("_Projectile_Anchor");
            PROJECTILE_ANCHOR = go.transform;
        }
        //查找父对象的fireDelegate
        GameObject parentGO = transform.parent.gameObject;
        if (parentGO.tag=="Hero")
        {
            Hero.S.fireDelegate += Fire;
        }
	}
   
    public WeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }
    public void SetType(WeaponType wt)
    {
        _type = wt;
        if (type==WeaponType.none)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }
        def = Main.GetWeaponDefinition(_type);
        collar.GetComponent<Renderer>().material.color = def.color;
        lastShot = 0;
    }

    public void Fire()
    {
        //如果this.gameObject处于未激活状态，则返回
        if (!gameObject.activeInHierarchy) return;
        //如果距离上次发射的时间间隔不足最小间隔，则返回
        if (Time.time-lastShot<def.delayBetweenShots)
        {
            return;
        }
        Projectile p;
        switch (type)
        {
            case WeaponType.none:
                break;
            case WeaponType.blaster:
                p = MakeProjectile();
                p.GetComponent<Rigidbody>().velocity = Vector3.up * def.velocity;
                break;
            case WeaponType.spread:
                p = MakeProjectile();
                p.GetComponent<Rigidbody>().velocity = Vector3.up * def.velocity;
                p = MakeProjectile();
                p.GetComponent<Rigidbody>().velocity = new Vector3(-0.2f,0.9f,0) * def.velocity;
                p = MakeProjectile();
                p.GetComponent<Rigidbody>().velocity = new Vector3(0.2f,0.9f,0) * def.velocity;
                break;
            case WeaponType.phaser:
                break;
            case WeaponType.missile:
                break;
            case WeaponType.laser:
                break;
            case WeaponType.shield:
                break;
        }
    }

    public Projectile MakeProjectile()
    {
        GameObject go = Instantiate(def.projectilePrefab) as GameObject;
        if (transform.parent.gameObject.tag=="Hero")
        {
            go.tag = "ProjectileHero";
            go.layer = LayerMask.NameToLayer("ProjectileHero");
        }
        else
        {
            go.tag = "ProjectileEnemy";
            go.layer = LayerMask.NameToLayer("ProjectileEnemy");
        }
        go.transform.position = collar.transform.position;
        go.transform.parent = PROJECTILE_ANCHOR;
        Projectile p = go.GetComponent<Projectile>();
        p.type = type;
        lastShot = Time.time;
        return (p);
    }
}
