using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base idea from https://www.youtube.com/watch?v=eMpI1eCsIyM
public class FishSwarm : MonoBehaviour
{
    public GameObject fishPrefab;

    public float spawnRange = 1.5f;
    public int numFish = 10;

    [HideInInspector]
    public GameObject[] allFish;

    private Vector3 goalPos = Vector3.zero;

    void Start()
    {
        allFish = new GameObject[numFish];
    }

    IEnumerator UpdatePosition()
    {
        while (true)
        {
            SetGoalPosition();

            float seconds = Random.Range(0.5f, 5.0f);
            yield return new WaitForSeconds(seconds);
        }
    }
    

    void SetGoalPosition()
    {
        Vector3 center = transform.position;

        goalPos = center + new Vector3(
            Random.Range(-spawnRange, spawnRange),
            Random.Range(-spawnRange, spawnRange),
            Random.Range(-spawnRange, spawnRange)
        );
    }

    public Vector3 GetGoalPosition()
    {
        return goalPos;
    }

    public void CleanUp()
    {
        foreach (var fish in allFish)
        {
            Destroy(fish);
        }

        StopAllCoroutines();
    }

    public void SetUp()
    {
        Vector3 center = transform.position;

        for (int i = 0; i < numFish; i++)
        {
            Vector3 pos = center + new Vector3(
                Random.Range(-spawnRange, spawnRange),
                Random.Range(-spawnRange, spawnRange),
                Random.Range(-spawnRange, spawnRange)
            );

            Quaternion rot = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));

            allFish[i] = (GameObject)Instantiate(fishPrefab, pos, rot);
        }

        StartCoroutine(UpdatePosition());
    }
}
