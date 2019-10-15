using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour {
    //使用二维向量Vector2的x存储Random.Ranger最小值
    public Vector2 rotMinMax = new Vector2(15, 90);
    public Vector2 driftMinMax = new Vector2(0.25f, 2);
    public float lifeTime = 6f;//升级道具存在的时间长度
    public float fadeTime = 4f;//升级道具渐隐所用的时间
    public WeaponType type;//升级道具的类型
    public GameObject cube;//对立方体子对象的引用
    public TextMesh letter;//对文本网格的引用
    public Vector3 rotPerSecond;//欧拉旋转的速度
    public float birthTime;
    private int u;

    void Awake()
    {
        //设置对立方体的引用
        cube = transform.Find("Cube").gameObject;
        //设置对文本网格的引用
        letter = GetComponent<TextMesh>();
        //设置一个随机速度
        Vector3 vel = Random.onUnitSphere;//获取一个随机的xyz速度
        //使用Random.onUnitSphere，可以获得一个以原点为球心、
        //1米为半径的球体表面上的一个点
        vel.z = 0;//使速度方向处于xy平面上
        vel.Normalize();//使速度大小变为1
        //三维向量的Normalize方法将使它的长度变为1
        vel *= Random.Range(driftMinMax.x, driftMinMax.y);
        //上述代码将速度设置为driftMinMax的x、y值之间的一个值
        GetComponent<Rigidbody>().velocity = vel;
        //将本游戏对象的旋转设置为[0,0,0];
        transform.rotation = Quaternion.identity;
        //Quaternion.identity的旋转为0
        //使用rotMinMax的x、y值设置立方体子对象每秒旋转圈数rotPerSecond
        rotPerSecond = new Vector3(Random.Range(rotMinMax.x, rotMinMax.y), Random.Range(rotMinMax.x, rotMinMax.y), Random.Range(rotMinMax.x, rotMinMax.y));
        //每隔两秒调用一次检查是否处于屏幕之外
        InvokeRepeating("CheckOffscreen", 2f, 2f);
        birthTime = Time.time;
    }
    
	void Update () {
        cube.transform.rotation = Quaternion.Euler(rotPerSecond * Time.time);
        if (u>=1)
        {
            Destroy(this.gameObject);
            return;
        }
        if (u>0)
        {
            Color c = cube.GetComponent<Renderer>().material.color;
            c.a = 1f - u;
            cube.GetComponent<Renderer>().material.color = c;
            c = letter.color;
            c.a = 1f - (u * 0.5f);
            letter.color = c;
        }
	}

    public void SetType(WeaponType wt)
    {
        //从Main脚本中获取WeaponDefinition值
        WeaponDefinition def = Main.GetWeaponDefinition(wt);
        //设置立方体子对象的颜色
        cube.GetComponent<Renderer>().material.color = def.color;
        //letter.color = def.color;//我们也可以给字母上色
        letter.text = def.letter;//设置显示的颜色
        type = wt;//最后设置升级道具的类型
    }

    public void AbsorbedBy(GameObject target)
    {
        //Hero类在收集到道具之后调用本函数
        //我们可以让升级道具逐渐缩小，吸收到目标对象中
        //但现在，简单消除this.gameObject即可
        Destroy(this.gameObject);
    }

    void CheckOffscreen()
    {
        //如果道具完全飘出屏幕
        if (Utils.ScreenBoundsCheck(cube.GetComponent<Collider>().bounds,BoundsTest.offScreen)!=Vector3.zero)
        {
            Destroy(this.gameObject);
        }
    }
}
