using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public enum Augmentation { water, fire };

public class AugmentationManager : MonoBehaviour
{
    public bool Active
    {
        get { return active; }
        set
        {
            if (active == value) return;
            active = value;
            if (OnActiveChange != null)
            {
                OnActiveChange(active);
            }
        }
    }
    private bool active;
    public delegate void OnActiveChangeDelegate(bool on);
    public event OnActiveChangeDelegate OnActiveChange;

    public Augmentation CurrentAugmentation
    {
        get { return currentAugmentation; }
        set
        {
            if (currentAugmentation == value) return;
            if (OnAugmentationChange != null)
            {
                OnAugmentationChange(currentAugmentation, value);
            }
            currentAugmentation = value;
        }
    }
    private Augmentation currentAugmentation;
    public delegate void OnAugmentationChangeDelegate(Augmentation oldVal, Augmentation newVal);
    public event OnAugmentationChangeDelegate OnAugmentationChange;

    [Header("Water")]
    public Material ppUnderwater;
    public FishSwarm fishManager;
    public ParticleSystem[] particleSystems;

    [Header("Fire")]
    public Material ppFire;

    private PostProcess postProcess;

    private void Start()
    {
        postProcess = Camera.main.GetComponent<PostProcess>();

        Active = false;
        CurrentAugmentation = Augmentation.water;

        OnActiveChange += SwitchActive;
        OnAugmentationChange += SwitchAugmentation;
    }

    // Callback function that triggers whenever the active property of this script is changed
    private void SwitchActive(bool on)
    {
        if (on)
        {
            ActivateAugmentation(CurrentAugmentation);
        }
        else
        {
            DeactivateAugmentation();
        }
    }

    // Callback function that triggers whenever the currentAugmentation property of this script is changed
    public void SwitchAugmentation(Augmentation oldVal, Augmentation newVal)
    {
        if (Active)
        {
            DeactivateAugmentation();
            ActivateAugmentation(newVal);
        }
    }

    // Set up and start all needed effects for the augmentation
    private void ActivateAugmentation(Augmentation type)
    {
        switch (type)
        {
            case Augmentation.water:
                postProcess.effectMaterial = ppUnderwater;
                fishManager.SetUp();
                foreach (var particles in particleSystems)
                {
                    particles.Play();
                }
                break;


            case Augmentation.fire:
                postProcess.effectMaterial = ppFire;
                break;
        }
    }

    // Clear and stop all used effects
    private void DeactivateAugmentation()
    {
        // TODO heat effects
        fishManager.CleanUp();
        foreach (var particles in particleSystems)
        {
            particles.Clear();
            particles.Stop();
        }
    }
}
