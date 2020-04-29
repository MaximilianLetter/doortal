using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EnterPortal : MonoBehaviour
{
    private Transform device;
    private PortalManager portalManager;

    // wasInFront logic should not be necessary for doorway interaction
    bool wasInFront;

    bool hasCollided;

    void Start()
    {
        device = GameObject.FindWithTag("MainCamera").transform;
        portalManager = GameObject.Find("PortalManager").GetComponent<PortalManager>();
    }

    bool GetDeviceInFront()
    {
        // Adjust clipping bug
        Vector3 worldPos = device.position + device.forward * Camera.main.nearClipPlane;

        // Position of device relative to the portal
        Vector3 pos = transform.InverseTransformPoint(worldPos);
        return pos.z >= 0 ? true : false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform != device) return;

        wasInFront = GetDeviceInFront();
        hasCollided = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform != device) return;

        hasCollided = false;
    }

    void WhileColliding()
    {
        // Adjust flickering bug, not completely solved tho

        if (!hasCollided) return;

        bool isInFront = GetDeviceInFront();

        if ((isInFront && !wasInFront) || (wasInFront && !isInFront))
        {
            portalManager.EnterPortal();
        }
        wasInFront = isInFront;
    }

    private void Update()
    {
        // NOTE: Could be coroutine?
        WhileColliding();
    }
}
