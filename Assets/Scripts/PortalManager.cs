using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PortalManager : MonoBehaviour
{
    [Header("Spawn properties")]
    public GameObject objectToSpawn;

    public float spawnTime;
    public float replacementDistance;

    [Header("Particle properties")]
    public GameObject augmentationCenter;
    public Transform device;

    [Header("Portal properties")]
    public Material[] materials;

    private AugmentationManager augmentationManager;

    private GameObject activePortal;
    private bool inside;
    private bool created;

    private void Start()
    {
        inside = false;
        created = false;

        SetMaterials(false);
        augmentationManager = augmentationCenter.GetComponent<AugmentationManager>();
    }

    // Hack for editor because of missing reset after play mode
    void OnDestroy()
    {
        SetMaterials(false);
    }

    // Function called from portals script when triggering walk through
    public void EnterPortal()
    {
        inside = !inside;
        SetMaterials(inside);

        if (inside)
        {
            // Add the particle emitter to the device so it will move with the player
            augmentationCenter.transform.parent = device;
            Vector3 position = augmentationCenter.transform.localPosition;
            augmentationCenter.transform.localPosition = new Vector3(position.x, position.y, 0);
        }
        else
        {
            // Leave the particle Emitter in the augmented world with an offset to the door
            Vector3 offset = activePortal.transform.rotation * new Vector3(0, 0, 1);

            augmentationCenter.transform.position = activePortal.transform.position + offset;
            augmentationCenter.transform.parent = null;
        }
    }

    private void SetMaterials(bool fullRender)
    {
        var stencilTest = fullRender ? CompareFunction.NotEqual : CompareFunction.Equal;

        foreach (var mat in materials)
        {
            mat.SetInt("_StencilTest", (int)stencilTest);
        }
    }

    public void SpawnObject(Vector3 position, Quaternion rotation, float width, float height)
    {
        if (activePortal == null)
        {
            GameObject obj = Instantiate(objectToSpawn, position, rotation);
            activePortal = obj;

            // Set the augmentationCenter active and place it in the door
            if (!inside)
            {
                Vector3 offset = rotation * new Vector3(0, 0, 1);
                augmentationCenter.transform.position = position + offset;
                augmentationCenter.transform.rotation = rotation;
            }
            else
            {
                // Make sure the door is always facing into the augmented world
                obj.transform.Rotate(Vector3.up, 180.0f);
            }

            if (!created)
            {
                augmentationManager.Active = true;
                created = true;
            }

            // Only scale up the portal window, so child objects are not stretched
            GameObject portalWindow = activePortal.transform.Find("PortalWindow").gameObject;
            Vector3 scale = new Vector3(width, height, 1);

            StartCoroutine(ScaleOverTime(portalWindow, spawnTime, scale));
        }
        else
        {
            float distance = Vector3.Distance(activePortal.transform.position, position);
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
            // Since only the portal window is scaled, the parent object needs to be destroyed
            Destroy(obj.transform.parent.gameObject);

            // Deactivate the augmentationCenter
            augmentationCenter.transform.parent = null;

            // Reset the scene to start
            SetMaterials(false);
            inside = false;

            augmentationManager.Active = false;
            created = false;
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
