using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CanvasLoader : MonoBehaviour
{   
    public enum FadeDirection {
        In, Out
    }
    public enum Scene {
        Intro, Mission, Command, Dialog, HoleDescent, Cube, Fps, None
    }
    public Canvas introCanvas;
    public Canvas consoleCanvas;
    public Canvas fadeCanvas;
    public Canvas fpsCanvas;
    public SkyboxManager skyboxManager;
    public void LoadSceneCanvas(Scene scene) {
        switch (scene) {
            case Scene.Intro:
                LoadIntroScene();
                break;
            case Scene.Mission:
                LoadMissionScene();
                break;
            case Scene.Command:
                LoadCommandScene();
                break;
            case Scene.Dialog:
                LoadDialogScene();
                break;
            case Scene.HoleDescent:
                LoadHoleDescent();
                break;
            case Scene.Cube:
                LoadCube();
                break;
            case Scene.Fps:
                LoadFps();
                break;
            case Scene.None:
                HideAllCanvas();
                break;
            default:
                break;            
        }
    }
    
    void HideAllCanvas() {
        introCanvas.gameObject.transform.Find("IntroBackground").gameObject.SetActive(false);
        introCanvas.gameObject.transform.Find("QuoteDisplayText").gameObject.SetActive(false);
        introCanvas.gameObject.transform.Find("MissionDisplayText").gameObject.SetActive(false);
        consoleCanvas.gameObject.transform.Find("ConsoleBackground").gameObject.SetActive(false);
        consoleCanvas.gameObject.transform.Find("ConsoleDisplayText").gameObject.SetActive(false);
        consoleCanvas.gameObject.transform.Find("ConsoleInputField").gameObject.SetActive(false);
    }

    void LoadIntroScene() {
        skyboxManager.setSpace();
        HideAllCanvas();
        introCanvas.gameObject.transform.Find("IntroBackground").gameObject.SetActive(true);
        introCanvas.gameObject.transform.Find("QuoteDisplayText").gameObject.SetActive(true);
    }

    void LoadMissionScene() {
        introCanvas.gameObject.transform.Find("IntroBackground").gameObject.SetActive(false);
        introCanvas.gameObject.transform.Find("QuoteDisplayText").gameObject.SetActive(false);
        introCanvas.gameObject.transform.Find("MissionDisplayText").gameObject.SetActive(true);
    }

    void LoadCommandScene() {
        HideAllCanvas();
        consoleCanvas.gameObject.transform.Find("ConsoleBackground").gameObject.SetActive(true);
        consoleCanvas.gameObject.transform.Find("ConsoleDisplayText").gameObject.SetActive(true);
    }

    void LoadDialogScene() {
        HideAllCanvas();
        consoleCanvas.gameObject.transform.Find("ConsoleBackground").gameObject.SetActive(true);
        consoleCanvas.gameObject.transform.Find("ConsoleDisplayText").gameObject.SetActive(true);
        consoleCanvas.gameObject.transform.Find("ConsoleInputField").gameObject.SetActive(true);
    }

    void LoadHoleDescent() {
        HideAllCanvas();
    }

    void LoadCube() {
        HideAllCanvas();
        skyboxManager.setDark();
    }

    void LoadFps() {
       fpsCanvas.gameObject.SetActive(true);
    }

    public IEnumerator Fade(FadeDirection fadeDirection, float fadeSpeed, Action<bool> done)
    {
        fadeCanvas.gameObject.transform.Find("FadeBackground").gameObject.SetActive(true);
        Image fadeBackground = fadeCanvas.gameObject.transform.Find("FadeBackground").gameObject.GetComponentInChildren<Image>();
        float alpha = (fadeDirection == FadeDirection.Out) ? 1 : 0;
        float fadeEndValue = (fadeDirection == FadeDirection.Out) ? 0 : 1;
        fadeBackground.enabled = true;
        if (fadeDirection == FadeDirection.Out)
        {
            while (alpha >= fadeEndValue)
            {
                SetColorImage(fadeBackground, ref alpha, fadeDirection, fadeSpeed);
                yield return null;
            }
            SetColorImage(fadeBackground, ref alpha, fadeDirection, fadeSpeed);
            fadeBackground.enabled = false;
            done(true);
        }
        else
        {
            while (alpha <= fadeEndValue)
            {
                SetColorImage(fadeBackground, ref alpha, fadeDirection, fadeSpeed);
                yield return null;
            }
            SetColorImage(fadeBackground, ref alpha, fadeDirection, fadeSpeed);
            done(true);
        }
    }

    void SetColorImage(Image fadeBackground, ref float alpha, FadeDirection fadeDirection, float fadeSpeed)
    {
        fadeBackground.color = new Color(fadeBackground.color.r, fadeBackground.color.g, fadeBackground.color.b, alpha);
        alpha += Time.deltaTime * (1.0f / fadeSpeed) * ((fadeDirection == FadeDirection.Out) ? -1 : 1);
    }
}
