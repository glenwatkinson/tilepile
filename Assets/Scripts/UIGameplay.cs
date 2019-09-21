using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplay : MonoBehaviour
{
    Text scoreText;
    Slider timeBar;

    void Awake()
    {
        scoreText = this.transform.Find("Score Text").GetComponent<Text>();
        timeBar = this.transform.Find("Time Bar").GetComponentInChildren<Slider>();
    }

    void Update()
    {
        scoreText.text = "" + GameManager.instance.score;
        timeBar.value = GameManager.instance.timeRemaining / GameManager.instance.maxTime;
    }
}
