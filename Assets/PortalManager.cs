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
        if (activePortal == null)
        {
            GameObject obj = Instantiate(objectToSpawn, position, rotation);
            activePortal = obj;

            // Only scale up the portal window, so child objects are not stretched
            GameObject portalWindow = activePortal.transform.Find("PortalWindow").gameObject;
            Vector3 scale = new Vector3(width, height, 1);

            StartCoroutine(ScaleOverTime(portalWindow, spawnTime, scale));
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
                activePortal = null; // Immidiate reset for recursive function call

                // Rerun the function to create a new portal
                SpawnObject(position, rotation, width, height);
            }
        }
    }

    private IEnumerator ScaleOverTime(GameObject obj, float time, Vector3 scale)
    {
        bool destroy = (scale == Vector3.zero);

        Vector3 originalScale;

        if (!destroy)
        {
            obj.transform.localPosition = new Vector3(0, scale.y / 2, 0);

            originalScale = Vector3.zero;
        }
        else
        {
            originalScale = obj.transform.localScale;
        }

        float currentTime = 0.0f;

        do
        {
            obj.transform.localScale = Vector3.Lerp(originalScale, scale, currentTime / time);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);

        // Make sure the endresult is the destination
        obj.transform.localScale = scale;

        if (destroy)
        {
            // Since only the portal window is scaled, the whole portal object needs to be destroyed
            Destroy(activePortal);
        }
    }

    public void DestroyPortal()
    {
        if (activePortal == null) return;

        GameObject portalWindow = activePortal.transform.Find("PortalWindow").gameObject;

        StartCoroutine(ScaleOverTime(portalWindow, spawnTime, Vector3.zero));
    }

    private void ReplacePortal(Vector3 position, Quaternion rotation, float width, float height)
    {
        activePortal.transform.SetPositionAndRotation(position, rotation);

        GameObject portalWindow = activePortal.transform.Find("PortalWindow").gameObject;
        portalWindow.transform.localScale = new Vector3(width, height, 1);
    }
}
