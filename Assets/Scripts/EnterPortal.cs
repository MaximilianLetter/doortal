using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EnterPortal : MonoBehaviour
{
    public Material[] materials;

    private Transform device;

    // wasInFront logic should not be necessary for doorway interaction
    bool wasInFront;
    bool inside;

    bool hasCollided;

    // Hack for editor setup
    void Start()
    {
        device = GameObject.FindWithTag("MainCamera").transform;
        SetMaterials(false);
    }

    void SetMaterials(bool fullRender)
    {
        var stencilTest = fullRender ? CompareFunction.NotEqual : CompareFunction.Equal;

        foreach (var mat in materials)
        {
            mat.SetInt("_StencilTest", (int)stencilTest);
        }
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
            inside = !inside;
            SetMaterials(inside);
        }
        wasInFront = isInFront;
    }

    // Hack for editor because of missing reset after play mode
    void OnDestroy()
    {
        SetMaterials(true);
    }

    private void Update()
    {
        // NOTE: Could be coroutine?
        WhileColliding();
    }
}
