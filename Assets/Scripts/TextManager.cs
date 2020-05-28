using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public enum TextContent { scanGround, noGround, doorCleared, everythingCleared };

public class TextManager : MonoBehaviour
{
    public float displayTime = 2.0f;

    private Text content;

    void Start()
    {
        content = GetComponent<Text>();

        HideNotification();
    }

    public void ShowNotification(TextContent type)
    {
        gameObject.SetActive(true);
        switch (type)
        {
            case TextContent.scanGround:
                content.text = "For setup, scan the ground by moving your device until the floor is detected.";
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
                Invoke("HideNotification", displayTime);
                break;
        }

    }

    private void HideNotification()
    {
        content.text = "";
        gameObject.SetActive(false);
    }
}
