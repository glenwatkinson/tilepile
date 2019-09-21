using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    CanvasGroup[] uiSection;

    void Awake()
    {
        uiSection = new CanvasGroup[System.Enum.GetValues(typeof(GameState)).Length];
        uiSection[(int)GameState.Opening] = this.transform.Find("Opening").GetComponent<CanvasGroup>();
        uiSection[(int)GameState.Gameplay] = this.transform.Find("Gameplay").GetComponent<CanvasGroup>();
        uiSection[(int)GameState.GameOver] = this.transform.Find("Game Over").GetComponent<CanvasGroup>();
    }

    public void SetUIScreen(GameState newState)
    {
        for (int a = 0; a < uiSection.Length; a++)
        {
            if (uiSection[a] != null)
            {
                if (a == (int)newState)
                {
                    uiSection[a].gameObject.SetActive(true);
                    if (uiSection[a].GetComponent<IUIScreen>() != null)
                        uiSection[a].GetComponent<IUIScreen>().ConfigureScreen();
                }
                else
                {
                    uiSection[a].gameObject.SetActive(false);
                }
            }
        }
    }
}
