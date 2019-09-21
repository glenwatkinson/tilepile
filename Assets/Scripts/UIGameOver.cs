using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOver : MonoBehaviour, IUIScreen
{
    Text scoreText;

    // Start is called before the first frame update
    void Awake()
    {
        scoreText = this.transform.Find("Score Text").GetComponent<Text>();
    }

    public void ConfigureScreen()
    {
        scoreText.text = "" + GameManager.instance.score;
    }
}
