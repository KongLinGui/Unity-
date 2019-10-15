using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {

    public float rotationsPerSecond = 0.1f;
    public int levelShown = 0;

	void Update () {
        //读取Hero单例对象的当前护盾等级
        int currLevel = Mathf.FloorToInt(Hero.S.shieldLevel);
        //如果当前护盾等级与显示的等级不符
        if (levelShown!=currLevel)
        {
            levelShown = currLevel;
            Material mat = GetComponent<Renderer>().material;
            //则调整纹理偏移量，呈现正确的护盾画面
            mat.mainTextureOffset = new Vector2(0.2f * levelShown, 0);
        }
        //每秒钟将护盾旋转一定角度
        float rZ = (rotationsPerSecond * Time.time * 360) % 360f;
        transform.rotation = Quaternion.Euler(0, 0, rZ);
	}
}
