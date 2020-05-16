using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentationChangeButton : MonoBehaviour
{
    public Augmentation augmentation;

    private AugmentationManager augmentationManager;

    private void Start()
    {
        augmentationManager = GameObject.FindObjectOfType<AugmentationManager>();
    }

    public void ChangeAugmentation()
    {
        augmentationManager.CurrentAugmentation = augmentation;
    }
}
