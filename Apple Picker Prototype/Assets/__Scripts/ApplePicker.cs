using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplePicker : MonoBehaviour {
    public GameObject basketPrefab;
    public int numBaskets = 3;
    public float basketBottomY = -14f;
    public float basketSpacingY = 2f;
    public List<GameObject> basketList;

	void Start () {
        basketList = new List<GameObject>();
        for (int i = 0; i < numBaskets; i++)
        {
            GameObject tBasketGO = Instantiate(basketPrefab) as GameObject;
            Vector3 pos = Vector3.zero;
            pos.y = basketBottomY + (basketSpacingY * i);
            tBasketGO.transform.position = pos;
            basketList.Add(tBasketGO);
        }
	}	

    public void AppleDestroyed()
    {
        //消除所有下落中的苹果
        GameObject[] tAppleArray = GameObject.FindGameObjectsWithTag("Apple");
        foreach (GameObject tGO in tAppleArray)
        {
            Destroy(tGO);
        }
        //消除一个篮筐
        //获取basketList中最后一个篮筐的序号
        int basketIndex = basketList.Count - 1;
        //取得对该篮筐的引用
        GameObject tBasketGO = basketList[basketIndex];
        //从列表中清除该篮筐并销毁该游戏对象
        basketList.RemoveAt(basketIndex);
        Destroy(tBasketGO);
        //重新开始游戏，HighScore.score不会受到影响
        if (basketList.Count==0)
        {
            SceneManager.LoadScene("_Scene_0");
        }
    }
}
