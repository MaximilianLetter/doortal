using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageToWorldDev : MonoBehaviour
{
    public Transform spawnTransform;
    public PortalManagerDev portalManager;

    public GameObject colliderBox;

    private const float width = 1.0f;
    private const float height = 2.2f;

    public void PlaceObject()
    {
        Vector3 boxPosition = spawnTransform.position + new Vector3(0, height / 2);

        GameObject colliderObj = Instantiate(colliderBox, boxPosition, spawnTransform.rotation);
        colliderObj.transform.localScale = new Vector3(width, height, 1);

        portalManager.SpawnObject(spawnTransform.position, spawnTransform.rotation, width, height);
    }

    public void ClearObjects()
    {
        portalManager.DestroyPortal();
    }
}
