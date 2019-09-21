using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BtnStateChange : MonoBehaviour
{
    public GameState changeTo;

    void Awake()
    {
        this.GetComponent<Button>().onClick.AddListener(() => GameManager.instance.SetGameState(changeTo));
    }
}
