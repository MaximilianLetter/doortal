﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public enum TextContent {
    notSupported,
    scanGround,
    onboardingComplete,
    noGround,
    doorCleared,
    everythingCleared
};

public class TextManager : MonoBehaviour
{
    public float displayTime = 2.0f;
    public float displayTimeLong = 3.0f;

    public GameObject textObject;
    private Text content;

    void Start()
    {
        content = textObject.GetComponent<Text>();

        HideNotification();
    }

    public void ShowNotification(TextContent type)
    {
        textObject.SetActive(true);
        switch (type)
        {
            case TextContent.notSupported:
                content.text = "Augmented Reality is not supported on your device.";
                break;
            case TextContent.scanGround:
                content.text = "For setup, scan the ground by moving your device until the floor is detected.";
                break;
            case TextContent.onboardingComplete:
                content.text = "Ground was detected.\nGo find a door to open a portal.";
                Invoke("HideNotification", displayTimeLong);
                break;
            case TextContent.noGround:
                content.text = "No ground was found.\nMake sure the ground near the door is detected.";
                Invoke("HideNotification", displayTime);
                break;
            case TextContent.doorCleared:
                content.text = "Cleared the current augmentation.";
                Invoke("HideNotification", displayTime);
                break;
            case TextContent.everythingCleared:
                content.text = "Reset everything, new ground planes need to be detected.";
                Invoke("HideNotification", displayTimeLong);
                break;
        }

    }

    private void HideNotification()
    {
        content.text = "";
        textObject.SetActive(false);
    }
}