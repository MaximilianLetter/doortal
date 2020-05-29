using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public enum Augmentation { water, fire, snow, pixel };

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

    public bool moveWithDevice;

    [Header("Water")]
    public Material ppUnderwater;
    public FishSwarm fishManager;
    public ParticleSystem[] particleSystemsWater;

    [Header("Fire")]
    public Material ppFire;
    public ParticleSystem[] particleSystemsFire;
    public RandomMovement randomSmoke;

    [Header("Snow")]
    public Material ppSnow;
    public ParticleSystem[] particleSystemsSnow;

    [Header("Pixel")]
    public Material ppPixel;

    private GameObject device;
    private PostProcess postProcess;

    private void Start()
    {
        device = Camera.main.gameObject;
        postProcess = Camera.main.GetComponent<PostProcess>();

        moveWithDevice = false;
        Active = false;
        CurrentAugmentation = Augmentation.water;

        OnActiveChange += SwitchActive;
        OnAugmentationChange += SwitchAugmentation;
    }

    private void FixedUpdate()
    {
        if (moveWithDevice)
        {
            Vector3 pos = device.transform.position;
            Vector3 current = gameObject.transform.position;

            // Only alter x and z position, height remains on ground level
            gameObject.transform.position = new Vector3(pos.x, current.y, pos.z);
        }
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
                foreach (var particles in particleSystemsWater)
                {
                    particles.Play();
                }
                break;


            case Augmentation.fire:
                postProcess.effectMaterial = ppFire;
                randomSmoke.SetUp();
                foreach (var particles in particleSystemsFire)
                {
                    particles.Play();
                }
                break;

            case Augmentation.snow:
                postProcess.effectMaterial = ppSnow;
                //randomSmoke.SetUp();
                foreach (var particles in particleSystemsSnow)
                {
                    particles.Play();
                }
                break;

            case Augmentation.pixel:
                postProcess.effectMaterial = ppPixel;
                break;
        }
    }

    // Clear and stop all used effects
    private void DeactivateAugmentation()
    {
        fishManager.CleanUp();
        randomSmoke.CleanUp();

        foreach (var particles in particleSystemsWater)
        {
            particles.Clear();
            particles.Stop();
        }
        foreach (var particles in particleSystemsFire)
        {
            particles.Clear();
            particles.Stop();
        }
        foreach (var particles in particleSystemsSnow)
        {
            particles.Clear();
            particles.Stop();
        }
    }
}
