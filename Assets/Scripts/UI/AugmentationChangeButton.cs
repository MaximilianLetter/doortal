using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentationChangeButton : MonoBehaviour
{
    public Augmentation augmentation;

    private AugmentationManager augmentationManager;
    private ButtonManager buttonManager;

    private void Start()
    {
        augmentationManager = GameObject.FindObjectOfType<AugmentationManager>();
        buttonManager = transform.parent.gameObject.GetComponent<ButtonManager>();
    }

    public void ChangeAugmentation()
    {
        augmentationManager.CurrentAugmentation = augmentation;

        buttonManager.ResetButtons();

        GetComponent<RectTransform>().sizeDelta = buttonManager.activeButtonSize;
    }
}
