using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

interface IUIScreen
{
    void ConfigureScreen();
}

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
                    if (uiSection[a].GetComponent<IUIScreen>() != null)
                        uiSection[a].GetComponent<IUIScreen>().ConfigureScreen();
                    if (!uiSection[a].gameObject.activeSelf && uiSection[a].alpha == 0)
                        StartCoroutine(FadeInSection(uiSection[a]));
                }
                else
                {
                    if (uiSection[a].gameObject.activeSelf && uiSection[a].alpha == 1)
                        StartCoroutine(FadeOutSection(uiSection[a]));
                }
            }
        }
    }

    IEnumerator FadeInSection(CanvasGroup section)
    {
        section.gameObject.SetActive(true);
        section.alpha = 0;
        yield return new WaitForSeconds(0.25f);
        while (section.alpha < 1)
        {
            section.alpha += Time.deltaTime * 4.0f;
            yield return null;
        }
    }

    IEnumerator FadeOutSection(CanvasGroup section)
    {
        section.alpha = 1.0f;
        while (section.alpha > 0)
        {
            section.alpha -= Time.deltaTime * 4.0f;
            yield return null;
        }
        section.gameObject.SetActive(false);
    }
}
