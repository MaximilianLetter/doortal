using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSample : MonoBehaviour
{
    private ParticleSystem ps;

    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();

        StartCoroutine(SampleParticleRoutine());
    }

    IEnumerator SampleParticleRoutine()
    {
        var main = ps.main;
        main.simulationSpeed = 1000f;
        ps.Play();

        yield return new WaitForSeconds(0.1f);

        main.simulationSpeed = 0.05f;
    } 
}
