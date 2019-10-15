using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour {
    static public Hero S;//单例对象
    public float gameRestartDelay = 2f;
    //以下字段用来控制飞船的运动
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;

    //飞船状态信息
    [SerializeField]
    private float _shieldLevel = 1;
    public Bounds bounds;
    //声明一个新的委托类型WeaponFireDelegate
    public delegate void WeaponFireDelegate();
    //创建一个名为fireDelegate的WeaponFireDelegate类型字段
    public WeaponFireDelegate fireDelegate;
    //Weapon(武器)字段
    public Weapon[] weapons;
    void Awake()
    {
        S = this;//设置单例对象
        bounds = Utils.CombineBoundsOfChildren(this.gameObject);
        
    }
	
    void Start()
    {
        //重置武器，让主角飞船从1个高爆弹武器开始
        ClearWeapons();
        weapons[0].SetType(WeaponType.blaster);
    }

	void Update () {
        //从input（用户输入）类中获取信息
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");
        //基于获取的水平轴和竖直轴信息修改 transform.position
        Vector3 pos = transform.position;
        pos.x += xAxis * speed * Time.deltaTime;
        pos.y += yAxis * speed * Time.deltaTime;
        transform.position = pos;

        bounds.center = transform.position;
        //使飞船保持在屏幕边界内
        Vector3 off = Utils.ScreenBoundsCheck(bounds, BoundsTest.onScreen);
        if (off!=Vector3.zero)
        {
            pos -= off;
            transform.position = pos;
        }

        //让飞船旋转一个角度，使它更具动感
        transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);
        //使用fireDelegate委托发射武器
        //首先，确认玩家按下了Axis("Jump")按钮
        //然后确认fireDelegate不为null，避免产生错误
        if (Input.GetAxis("Jump")==1&&fireDelegate!=null)
        {
            fireDelegate();
        }
	}

    //此变量用于存储最后一次触发护盾碰撞器的游戏对象
    public GameObject lastTriggerGo = null;

    void OnTriggerEnter(Collider other)
    {
        //查找other.gameObject或其父对象的标签
        GameObject go = Utils.FindTaggedParent(other.gameObject);
        //如果存在具有自定义标签的父对象
        if (go!=null)
        {            
            //确保此次触发碰撞事件的对象与上次不同
            if (go==lastTriggerGo)
            {
                return;
            }

            lastTriggerGo = go;
            if (go.tag=="Enemy")
            {
                //如果护盾被敌机触发
                //则让护盾下降一个等级
                shieldLevel--;
                //消灭敌机
                Destroy(go);
            }
            else if (go.tag=="PowerUp")
            {
                //如果护盾被升级道具触发
                AbsorbPowerUp(go);
            }
            else
            {
                //则声明其名称
                print("触发碰撞事件：" + go.name);
            }
        }
        else
        {
            //否则，则声明other.gameObject的名称
            print("触发碰撞事件：" + other.gameObject.name);
        }        
    }

    public void AbsorbPowerUp(GameObject go)
    {
        PowerUp pu = go.GetComponent<PowerUp>();
        switch (pu.type)
        {
            case WeaponType.none:
                break;
            case WeaponType.blaster:
                break;
            case WeaponType.spread:
                break;
            case WeaponType.phaser:
                break;
            case WeaponType.missile:
                break;
            case WeaponType.laser:
                break;
            case WeaponType.shield:
                shieldLevel++;
                break;
            default://如果是任何一种武器升级道具
                //检查当前武器类型
                if (pu.type==weapons[0].type)
                {
                    //增加该类型武器的数量
                    Weapon w = GetEmptyWeaponSlot();//找到一个空白武器位置
                    if (w!=null)
                    {
                        //将其赋值给pu.type
                        w.SetType(pu.type);
                    }
                }
                else
                {
                    //如果武器类型不一样
                    ClearWeapons();
                    weapons[0].SetType(pu.type);
                }
                break;
        }
        pu.AbsorbedBy(this.gameObject);
    }

    Weapon GetEmptyWeaponSlot()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].type==WeaponType.none)
            {
                return (weapons[i]);
            }
        }
        return (null);
    }

    void ClearWeapons()
    {
        foreach (Weapon w in weapons)
        {
            w.SetType(WeaponType.none);
        }
    }

    public float shieldLevel
    {
        get
        {
            return (_shieldLevel);
        }
        set
        {
            _shieldLevel = Mathf.Min(value, 4);
            //如果护盾等级小于0
            if (value<0)
            {
                Destroy(this.gameObject);
                //通知Main.S延时重新开始游戏
                Main.S.DelayedRestart(gameRestartDelay);
            }
        }
    }
}
