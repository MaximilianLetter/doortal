using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageToWorldDev : MonoBehaviour
{
    public GameObject objectToSpawn;
    public Transform spawnTransform;

    public const float spawnTime = 0.2f;

    private const float width = 1.0f;
    private const float height = 2.2f;

    public void SpawnObject()
    {
        GameObject obj = Instantiate(objectToSpawn, spawnTransform.position, spawnTransform.rotation);

        obj.transform.localScale = new Vector3(0.01f, 0.01f, 1);
        StartCoroutine(ScaleOverTime(obj, spawnTime, width, height));
    }

    private IEnumerator ScaleOverTime(GameObject obj, float time, float width, float height)
    {
        Vector3 originalScale = obj.transform.localScale;
        Vector3 destinationScale = new Vector3(width, height, 1.0f);

        float currentTime = 0.0f;

        do
        {
            obj.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / time);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);

        // Make sure the endresult is the destination
        obj.transform.localScale = destinationScale;
    }

    public void ClearObjects()
    {
        Debug.Log("clear all portals");
    }
}
