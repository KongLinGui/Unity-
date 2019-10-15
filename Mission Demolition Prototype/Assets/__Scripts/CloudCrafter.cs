using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudCrafter : MonoBehaviour {
    //在unity检视面板中设置的字段
    public int numClouds = 40;//要创建云朵的数量
    public GameObject[] cloudPrefabs;//云朵预设的数组
    public Vector3 cloudPosMin;//云朵位置的下限
    public Vector3 cloudPosMax;//云朵位置的上限
    public float cloudScaleMin = 1;//云朵的最小缩放比例
    public float cloudScaleMax = 5;//云朵的最大缩放比例
    public float cloudSpeedMult = 0.5f;//调整云朵速度
    //动态设置的字段
    public GameObject[] cloudInstances;

    void Awake()
    {
        //创建一个cloudInstances数组，用于存储所有云朵的实例
        cloudInstances = new GameObject[numClouds];
        //查找CloudAnchor父对象
        GameObject anchor = GameObject.Find("CloudAnchor");
        //遍历cloud_[]并创建实例
        GameObject cloud;
        for (int i = 0; i < numClouds; i++)
        {
            //在0到cloudPrefabs.Length-1之间选择一个整数
            //Random.Range返回值中不包含范围上限
            int prefabNum = Random.Range(0, cloudPrefabs.Length);
            //创建一个实例
            cloud = Instantiate(cloudPrefabs[prefabNum]) as GameObject;
            //设置云朵位置
            Vector3 cPos = Vector3.zero;
            cPos.x = Random.Range(cloudPosMin.x, cloudPosMax.x);
            cPos.y = Random.Range(cloudPosMin.y, cloudPosMax.y);
            //设置云朵缩放比例
            float scaleU = Random.value;
            float scaleVal = Mathf.Lerp(cloudScaleMin, cloudScaleMax, scaleU);
            //较小的云朵离地面较近
            cPos.y = Mathf.Lerp(cloudPosMin.y, cPos.y, scaleU);
            //较小的云朵距离较远
            cPos.z = 100 - 90 * scaleU;
            //将上述变换数值应用到云朵
            cloud.transform.position = cPos;
            cloud.transform.localScale = Vector3.one * scaleVal;
            //使云朵成为CloudAnchor的子对象
            cloud.transform.parent = anchor.transform;
            //将云朵添加到cloudInstances数组中
            cloudInstances[i] = cloud;
        }
    }

	void Update () {
        //遍历所有已创建的云朵
        foreach (GameObject cloud in cloudInstances)
        {
            //获取云朵的缩放比例和位置
            float scaleVal = cloud.transform.localScale.x;
            Vector3 cPos = cloud.transform.position;
            //云朵越大，移动速度越快
            cPos.x -= scaleVal * Time.deltaTime * cloudSpeedMult;
            //如果云朵已经位于画面左侧较远位置
            if (cPos.x<=cloudPosMin.x)
            {
                //则将它放置到最右侧
                cPos.x = cloudPosMax.x;
            }
            //将新位置应用到云朵上
            cloud.transform.position = cPos;
        }
	}
}
