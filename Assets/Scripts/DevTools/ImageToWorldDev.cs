using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageToWorldDev : MonoBehaviour
{
    public Transform spawnTransform;
    public PortalManagerDev portalManager;

    private const float width = 1.0f;
    private const float height = 2.2f;

    public void PlaceObject()
    {
        portalManager.SpawnObject(spawnTransform.position, spawnTransform.rotation, width, height);
    }

    public void ClearObjects()
    {
        portalManager.DestroyPortal();
    }
}
