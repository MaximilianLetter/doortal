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

    // Hack for editor setup
    void Start()
    {
        device = GameObject.FindWithTag("MainCamera").transform;
        Debug.Log(device);
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
        // Position of device relative to the portal
        Vector3 pos = transform.InverseTransformPoint(device.position);
        return pos.z >= 0 ? true : false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform != device) return;

        wasInFront = GetDeviceInFront();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.transform != device) return;

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
}
