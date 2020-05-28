using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public Vector2 buttonSize = new Vector2(150, 150);
    public Vector2 activeButtonSize = new Vector2(300, 300);

    private RectTransform[] buttons;

    void Start()
    {
        buttons = new RectTransform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            buttons[i] = transform.GetChild(i).GetComponent<RectTransform>();
        }

        // Make sure all buttons are in default size
        ResetButtons();

        // Set the first element to the default active button
        buttons[0].sizeDelta = activeButtonSize;
    }

    public void ResetButtons()
    {
        foreach (var button in buttons)
        {
            button.sizeDelta = buttonSize;
        }
    }
}
