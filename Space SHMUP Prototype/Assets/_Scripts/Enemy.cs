using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    public float speed = 10f;       //运动速度，以m/s为单位
    public float fireRate = 0.3f;   //发射频率
    public float health = 10;
    public int score = 100;         //玩家击毁该敌机将得到的分数
    public int showDamageForFrames = 2;//显示伤害效果的帧数
    public float powerUpDropChance = 1f;//掉落升级道具的概率
    public Color[] originalColors;
    public Material[] materials;//本对象及其子对象的所有材质
    public int remainingDamageFrames = 0;//剩余的伤害效果帧数
    public Bounds bounds;           //本对象及其子对象的边界框
    public Vector3 boundsCenterOffset;//边界中心到position的距离

	void Awake()
    {
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }
        InvokeRepeating("CheckOffscreen", 0f, 2f);
    }

	void Update () {
        Move();
        if (remainingDamageFrames>0)
        {
            remainingDamageFrames--;
            if (remainingDamageFrames==0)
            {
                UnShowDamage();
            }
        }
	}

    //虚函数
    public virtual void Move()
    {
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    //pos是一个属性：即行为表现与字段相似的方法
    public Vector3 pos
    {
        get
        {
            return (this.transform.position);
        }
        set
        {
            this.transform.position = value;
        }
    }

    void CheckOffscreen()
    {
        //如果边界框为默认初始值
        if (bounds.size==Vector3.zero)
        {
            //则对其进行设置
            bounds = Utils.CombineBoundsOfChildren(this.gameObject);
            //同时检查bounds.center和transform.position之间的偏移
            boundsCenterOffset = bounds.center - transform.position;
        }
        //每次根据当前位置更新边界框
        bounds.center = transform.position + boundsCenterOffset;
        //检查边界框是否完全位于屏幕之外
        Vector3 off = Utils.ScreenBoundsCheck(bounds, BoundsTest.offScreen);
        if (off!=Vector3.zero)
        {
            //如果敌机超出屏幕底边界
            if (off.y<0)
            {
                //则销毁
                Destroy(this.gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        GameObject other = coll.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero":
                Projectile p = other.GetComponent<Projectile>();
                //在进入屏幕之前，敌机不会受到伤害
                //这可以避免玩家射击到屏幕外看不到的敌机
                bounds.center = transform.position + boundsCenterOffset;
                if (bounds.extents==Vector3.zero||Utils.ScreenBoundsCheck(bounds,BoundsTest.offScreen)!=Vector3.zero)
                {
                    Destroy(other);
                    break;
                }
                //给这架敌机造成伤害
                ShowDamage();
                //根据Projectile.type和Main.W_DEFS得出伤害值
                health -= Main.W_DEFS[p.type].damageOnHit;
                if (health<=0)
                {
                    //通知Main单例对象敌机已经被消灭
                    Main.S.ShipDestroyed(this);
                    //消灭该敌机
                    Destroy(this.gameObject);
                }
                Destroy(other);
                break;
        }
    }

    void ShowDamage()
    {
        foreach (Material m in materials)
        {
            m.color = Color.red;
        }
        remainingDamageFrames = showDamageForFrames;
    }

    void UnShowDamage()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].color = originalColors[i];
        }
    }
}
