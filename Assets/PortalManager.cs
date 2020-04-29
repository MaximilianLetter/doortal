using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public GameObject objectToSpawn;

    public const float spawnTime = 0.2f;

    private GameObject activePortal;

    public void SpawnObject(Vector3 position, Quaternion rotation, float width, float height)
    {
        // If no portal exists
        if (activePortal == null)
        {
            GameObject obj = Instantiate(objectToSpawn, position, rotation);

            obj.transform.localScale = new Vector3(0.01f, 0.01f, 1);

            StartCoroutine(ScaleOverTime(obj, spawnTime, width, height));

            activePortal = obj;
        }
        else
        {
            ReplacePortal(position, rotation, width, height);
        }
    }

    private IEnumerator ScaleOverTime(GameObject obj, float time, float width, float height, bool destroy = false)
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

        if (destroy)
        {
            Destroy(obj);
        }
    }

    public void DestroyPortal()
    {
        StartCoroutine(ScaleOverTime(activePortal, spawnTime, 0.01f, 0.01f, true));
    }

    private void ReplacePortal(Vector3 position, Quaternion rotation, float width, float height)
    {
        activePortal.transform.SetPositionAndRotation(position, rotation);
        activePortal.transform.localScale = new Vector3(width, height, 1);
    }
}
