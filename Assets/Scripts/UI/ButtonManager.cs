using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public Vector2 buttonSize = new Vector2(150, 150);
    public Vector2 activeButtonSize = new Vector2(300, 300);

    private RectTransform[] buttons;
    private int currentActive = 0;

    private AugmentationManager augmentationManager;

    void Start()
    {
        augmentationManager = GameObject.FindObjectOfType<AugmentationManager>();

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

    public void SetActiveButton(int num)
    {
        ResetButtons();

        buttons[num].sizeDelta = activeButtonSize;

        // Set active augmentation by casting the given number to Enumeration type
        augmentationManager.CurrentAugmentation = (Augmentation)num;

        currentActive = num;
    }

    public void SwipeAugmentation(int direction)
    {
        currentActive = Mathf.Clamp(currentActive - direction, 0, buttons.Length - 1);
        SetActiveButton(currentActive);
    }

    void ResetButtons()
    {
        foreach (var button in buttons)
        {
            button.sizeDelta = buttonSize;
        }
    }
}
