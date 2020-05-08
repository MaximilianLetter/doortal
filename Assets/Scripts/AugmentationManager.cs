using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentationManager : MonoBehaviour
{
    public GameObject fishManagerObj;
    public GameObject particleObj;

    private FishSwarm fishManager;
    private ParticleSystem particles;

    private void Start()
    {
        fishManager = fishManagerObj.GetComponent<FishSwarm>();
        particles = particleObj.GetComponent<ParticleSystem>();
    }

    public void DeactivateAugmentation()
    {
        fishManager.CleanUp();
        particles.Stop();
    }

    public void ActivateAugmentation()
    {
        fishManager.SetUp();
        particles.Play();
    }
}
