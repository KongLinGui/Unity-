using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour {
    static public Main S;
    static public Dictionary<WeaponType, WeaponDefinition> W_DEFS;
    public GameObject[] prefabEnemies;
    public float enemySpawnPerSecond = 0.5f;//每秒钟产生的敌机数量
    public float enemySpawnPadding = 1.5f;//敌机位置间隔
    public WeaponDefinition[] weaponDefinitions;
    public GameObject prefabPowerUp;
    public WeaponType[] powerUpFrequency = new WeaponType[]
    {
        WeaponType.blaster,
        WeaponType.blaster,
        WeaponType.spread,
        WeaponType.shield
    };
    public WeaponType[] activeWeaponTypes;
    public float enemySpawnRate;//生成敌机的时间间隔

    void Awake()
    {
        S = this;
        //设置Utils.camBounds
        Utils.SetCameraBounds(GetComponent<Camera>());
        //每秒钟产生0.5个敌人，即enemySpawnRate为2
        enemySpawnRate = 1f / enemySpawnPerSecond;
        //每延迟2秒调用一次SpawnEnemy()
        Invoke("SpawnEnemy", enemySpawnRate);
        W_DEFS = new Dictionary<WeaponType, WeaponDefinition>();
        foreach (WeaponDefinition def in weaponDefinitions)
        {
            W_DEFS[def.type] = def;
        }
    }

    static public WeaponDefinition GetWeaponDefinition(WeaponType wt)
    {
        if (W_DEFS.ContainsKey(wt))
        {
            return (W_DEFS[wt]);
        }
        return (new WeaponDefinition());
    }

    void Start()
    {
        activeWeaponTypes = new WeaponType[weaponDefinitions.Length];
        for (int i = 0; i < weaponDefinitions.Length; i++)
        {
            activeWeaponTypes[i] = weaponDefinitions[i].type;
        }
    }

	public void SpawnEnemy()
    {
        //随机选取一架敌机预设并实例化
        int ndx = Random.Range(0, prefabEnemies.Length);
        GameObject go = Instantiate(prefabEnemies[ndx] as GameObject);
        //将敌机置于屏幕上方，x坐标随机
        Vector3 pos = Vector3.zero;
        float xMin = Utils.camBounds.min.x + enemySpawnPadding;
        float xMax = Utils.camBounds.max.x - enemySpawnPadding;
        pos.x = Random.Range(xMin, xMax);
        pos.y = Utils.camBounds.max.y + enemySpawnPadding;
        go.transform.position = pos;
        //隔2秒后再次调用SpawnEnemy()
        Invoke("SpawnEnemy", enemySpawnRate);
    }

    public void DelayedRestart(float delay)
    {
        //延时调用Restart()方法，延时秒数为delay变量的值
        Invoke("Restart", delay);
    }

    public void Restart()
    {
        //重新加载场景_Scene_0，重新开始游戏
        SceneManager.LoadScene("_Scene_0");
    }

    public void ShipDestroyed(Enemy e)
    {
        //掉落升级道具的概率
        if (Random.value<=e.powerUpDropChance)
        {
            //Random.value生成一个0到1之间的数字（但不包括1）
            //如果e.powerUpDropChance的值为0.5f，则有50%的概率
            //产生升级道具。在测试时，这个值被设置为1f
            //选择要挑选哪个升级道具
            //从powerUpFrequency中选取其中一种可能
            int ndx = Random.Range(0, powerUpFrequency.Length);
            WeaponType puType = powerUpFrequency[ndx];
            //生成升级道具
            GameObject go = Instantiate(prefabPowerUp) as GameObject;
            PowerUp pu = go.GetComponent<PowerUp>();
            //将其设置为正确的武器类型
            pu.SetType(puType);
            //将其摆放在被敌机被消灭时的位置
            pu.transform.position = e.transform.position;
        }
    }
}
