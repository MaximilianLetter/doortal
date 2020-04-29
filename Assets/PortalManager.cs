using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public GameObject objectToSpawn;

    public float spawnTime;
    public float replacementDistance;

    private GameObject activePortal;

    public void SpawnObject(Vector3 position, Quaternion rotation, float width, float height)
    {
        // If no portal exists
        if (activePortal == null)
        {
            Vector3 startPosition = new Vector3(position.x, position.y + height / 2, position.z);
            Vector3 destinationScale = new Vector3(width, height, 1);
            GameObject obj = Instantiate(objectToSpawn, startPosition, rotation);

            obj.transform.localScale = new Vector3(0.01f, 0.01f, 1);

            StartCoroutine(CreationOverTime(obj, spawnTime ,destinationScale, position));

            activePortal = obj;
        }
        else
        {
            float distance = Vector2.Distance(activePortal.transform.position, position);
            if (distance <= replacementDistance)
            {
                ReplacePortal(position, rotation, width, height);
            }
            else
            {
                Destroy(activePortal);

                // This is a copy of above and could be outsourced
                Vector3 startPosition = new Vector3(position.x, position.y + height / 2, position.z);
                Vector3 destinationScale = new Vector3(width, height, 1);
                GameObject obj = Instantiate(objectToSpawn, startPosition, rotation);

                obj.transform.localScale = new Vector3(0.01f, 0.01f, 1);

                StartCoroutine(CreationOverTime(obj, spawnTime, destinationScale, position));

                activePortal = obj;
            }
        }
    }

    private IEnumerator CreationOverTime(GameObject obj, float time, Vector3 scale, Vector3 position, bool destroy = false)
    {
        Vector3 originalScale = obj.transform.localScale;
        Vector3 originalPosition = obj.transform.position;

        float currentTime = 0.0f;

        do
        {
            obj.transform.localScale = Vector3.Lerp(originalScale, scale, currentTime / time);
            obj.transform.position = Vector3.Lerp(originalPosition, position, currentTime / time);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);

        // Make sure the endresult is the destination
        obj.transform.localScale = scale;
        obj.transform.position = position;

        if (destroy)
        {
            Destroy(obj);
        }
    }

    public void DestroyPortal()
    {
        if (activePortal == null) return;

        Vector3 destinationScale = new Vector3(0.01f, 0.01f, 1);

        Vector3 position = activePortal.transform.position;
        float height = activePortal.transform.localScale.y;
        Vector3 destinationPosition = new Vector3(position.x, position.y + (height / 2), position.z);

        StartCoroutine(CreationOverTime(activePortal, spawnTime, destinationScale, destinationPosition, true));
    }

    private void ReplacePortal(Vector3 position, Quaternion rotation, float width, float height)
    {
        activePortal.transform.SetPositionAndRotation(position, rotation);
        activePortal.transform.localScale = new Vector3(width, height, 1);
    }
}
