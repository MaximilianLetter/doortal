using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PortalManager : MonoBehaviour
{
    [Header("Spawn properties")]
    public GameObject objectToSpawn;
    public GameObject doorMarker;

    public float spawnTime;
    public float replacementDistance;

    [Header("Particle properties")]
    public GameObject augmentationCenter;

    [Header("Portal properties")]
    public Material[] materials;

    private AugmentationManager augmentationManager;
    private ARAnchorManager anchorManager;

    private GameObject activePortal;
    private ARAnchor activeAnchor;
    private bool inside;
    private bool created;

    private void Start()
    {
        inside = false;
        created = false;

        SetMaterials(false);
        augmentationManager = augmentationCenter.GetComponent<AugmentationManager>();
        anchorManager = FindObjectOfType<ARAnchorManager>();
    }

    // Hack for editor because of missing reset after play mode
    void OnDestroy()
    {
        SetMaterials(false);
    }

    /// <summary>
    /// Function called from portals script when walking in or out. Modifing augmentation effects based on walking in or out.
    /// </summary>
    /// <returns>No return value.</returns>
    public void EnterPortal()
    {
        inside = !inside;
        SetMaterials(inside);

        if (inside)
        {
            // Let the particle emitter move with the player
            augmentationManager.moveWithDevice = true;
        }
        else
        {
            // Leave the particle Emitter in the augmented world
            augmentationManager.moveWithDevice = false;

            // Add an offset to the door position
            Vector3 offset = activePortal.transform.rotation * new Vector3(0, 0, 1);

            augmentationCenter.transform.position = activePortal.transform.position + offset;
        }
    }

    /// <summary>
    /// Sets the material stencil value for all assigned materials to either true or false.
    /// </summary>
    /// <param name="fullRender">Shall material value be true or false.</param>
    /// <returns>No return value.</returns>
    private void SetMaterials(bool fullRender)
    {
        var stencilTest = fullRender ? CompareFunction.NotEqual : CompareFunction.Equal;

        foreach (var mat in materials)
        {
            mat.SetInt("_StencilTest", (int)stencilTest);
        }
    }

    /// <summary>
    /// Spawn a portal at the given position. If a portal already exists, destroy the old one and create a new one.
    /// If the new portal is close to the existing portal, set new values for the existing portal.
    /// </summary>
    /// <param name="position">Position of a new AR Anchor that holds the position of the portal.</param>
    /// <param name="rotation">Rotation of the portal.</param>
    /// <param name="width">Width of the portal window.</param>
    /// <param name="height">Height of the portal window.</param>
    /// <returns>No return value.</returns>
    public void SpawnObject(Vector3 position, Quaternion rotation, float width, float height)
    {
        // Create an anchor that can be tracked from now on
        ARAnchor newAnchor = anchorManager.AddAnchor(new Pose(position, rotation));
        if (activeAnchor != null)
        {
            anchorManager.RemoveAnchor(activeAnchor);
        }
        activeAnchor = newAnchor;

        if (activePortal == null)
        {
            GameObject obj = Instantiate(objectToSpawn);
            obj.transform.parent = activeAnchor.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.rotation = rotation;

            activePortal = obj;

            // Set the augmentationCenter active and place behind the door
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

    /// <summary>
    /// Spawn a portal at the given position. If a portal already exists, destroy the old one and create a new one.
    /// If the new portal is close to the existing portal, set new values for the existing portal.
    /// </summary>
    /// <param name="position">Position of a new AR Anchor that holds the position of the portal.</param>
    /// <param name="rotation">Rotation of the portal.</param>
    /// <param name="width">Width of the portal window.</param>
    /// <param name="height">Height of the portal window.</param>
    /// <returns>No return value.</returns>
    public void SpawnPortal()
    {
        Vector3 position = doorMarker.transform.position;
        Quaternion rotation = doorMarker.transform.rotation;
        float width = doorMarker.transform.localScale.x;
        float height = doorMarker.transform.localScale.y;

        // Create an anchor that can be tracked from now on
        ARAnchor newAnchor = anchorManager.AddAnchor(new Pose(position, rotation));
        if (activeAnchor != null)
        {
            anchorManager.RemoveAnchor(activeAnchor);
        }
        activeAnchor = newAnchor;

        if (activePortal == null)
        {
            GameObject obj = Instantiate(objectToSpawn);
            obj.transform.parent = activeAnchor.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.rotation = rotation;

            activePortal = obj;

            // Set the augmentationCenter active and place behind the door
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

    /// <summary>
    /// Scales the given object over the given time. In the end it receives the given scale. If the given Scale is zero, the currently active portal gets destroyed.
    /// </summary>
    /// <param name="obj">Object to be scaled.</param>
    /// <param name="time">Length of coroutine.</param>
    /// <param name="scale">Scale that holds the width and the height the object shall receive. If zero, the currently active portal gets destroyed.</param>
    /// <returns>No return value.</returns>
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

        // Door detection needs more time than a normal frame
        // to display the full animation, wait until the next frame
        yield return null;

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

            // Remove the active anchor
            if (activeAnchor != null)
            {
                anchorManager.RemoveAnchor(activeAnchor);
            }

            // Deactivate the augmentationCenter
            augmentationCenter.transform.parent = null;

            // Reset the scene to start
            SetMaterials(false);
            inside = false;

            augmentationManager.Active = false;
            created = false;
        }
    }

    /// <summary>
    /// Destroys the current portal by calling a coroutine shrinking and deleting the object.
    /// </summary>
    /// <returns>No return value.</returns>
    public void DestroyPortal()
    {
        if (activePortal == null) return;

        GameObject portalWindow = activePortal.transform.Find("PortalWindow").gameObject;

        StartCoroutine(ScaleOverTime(portalWindow, spawnTime, Vector3.zero));
    }

    /// <summary>
    /// Replaces the current portal, creates a new AR Anchor and sets new position, rotation and scale.
    /// </summary>
    /// <param name="position">Position of a new AR Anchor that holds the position of the portal.</param>
    /// <param name="rotation">Rotation of the portal.</param>
    /// <param name="width">Width of the portal window.</param>
    /// <param name="height">Height of the portal window.</param>
    /// <returns>No return value.</returns>
    private void ReplacePortal(Vector3 position, Quaternion rotation, float width, float height)
    {
        // Create an anchor that can be tracked from now on
        ARAnchor newAnchor = anchorManager.AddAnchor(new Pose(position, rotation));
        if (activeAnchor != null)
        {
            anchorManager.RemoveAnchor(activeAnchor);
        }
        activeAnchor = newAnchor;

        activePortal.transform.parent = activeAnchor.transform;
        activePortal.transform.localPosition = Vector3.zero;
        activePortal.transform.rotation = rotation;

        GameObject portalWindow = activePortal.transform.Find("PortalWindow").gameObject;
        portalWindow.transform.localPosition = new Vector3(0, height / 2, 0);
        portalWindow.transform.localScale = new Vector3(width, height, 1);
    }
}
