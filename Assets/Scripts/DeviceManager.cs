using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceManager : MonoBehaviour
{
    public RenderTexture[] textures;

    // Awake is called even before Start()
    void Awake()
    {
        int width = Screen.width;
        int height = Screen.height;

        for (int i = 0; i < textures.Length; i++)
        {
            textures[i].Release();
            textures[i].width = width;
            textures[i].height = height;
            textures[i].Create();

            Debug.Log(textures[i].name + " set to: " + textures[i].width + ", " + textures[i].height);
        }
    }
}
