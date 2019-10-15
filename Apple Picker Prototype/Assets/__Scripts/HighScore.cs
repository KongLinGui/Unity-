using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScore : MonoBehaviour {

    static public int score = 1000;
    void Awake()
    {
        //如果ApplePickerHighScore已经存在，则读取其值
        if (PlayerPrefs.HasKey("ApplePickerHighScore"))
        {
            score = PlayerPrefs.GetInt("ApplePickerHighScore");
        }
        //将最高得分赋给ApplePickerHighScore
        PlayerPrefs.SetInt("ApplePickerHighScore", score);
    }

    void Update()
    {
        GUIText gt = this.GetComponent<GUIText>();
        gt.text = "High Score:" + score;
        //如有必要，则更新PlayerPrefs中的ApplePickerHighScore
        if (score>PlayerPrefs.GetInt("ApplePickerHighScore"))
        {
            PlayerPrefs.SetInt("ApplePickerHighScore", score);
        }
    }
}
