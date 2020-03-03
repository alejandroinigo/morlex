using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxManager : MonoBehaviour {
    public Material spaceSkybox;
    public Material darkSkybox;

    void Start () {
        RenderSettings.skybox = spaceSkybox;
    }

    public void setSpace() {
        RenderSettings.skybox = spaceSkybox;
    }

    public void setDark() {
        RenderSettings.skybox = darkSkybox;
    }
}