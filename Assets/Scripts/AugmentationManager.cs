using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentationManager : MonoBehaviour
{
    public FishSwarm fishManager;
    public ParticleSystem[] particleSystems;


    public void DeactivateAugmentation()
    {
        fishManager.CleanUp();
        foreach (var particles in particleSystems)
        {
            particles.Stop();
        }
    }

    public void ActivateAugmentation()
    {
        fishManager.SetUp();
        foreach (var particles in particleSystems)
        {
            particles.Play();
        }
    }
}
