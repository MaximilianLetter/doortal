using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[ExecuteInEditMode]
public class PostProcess : MonoBehaviour
{
    public Material effectMaterial;
    public Camera stencilCam;

    private RenderTexture stencilTex;

    private void Start()
    {
        stencilTex = new RenderTexture(Screen.width, Screen.height, 0)
        {
            format = RenderTextureFormat.R8,
            name = "stencilTex"
        };

        stencilCam.targetTexture = stencilTex;

        // Subscribe to state changed event to match the camera params
        ARSession.stateChanged += OnStageChanged;
    }

    void OnStageChanged(ARSessionStateChangedEventArgs stateChange)
    {
        if (stateChange.state == ARSessionState.SessionTracking)
        {
            Camera cam = Camera.main;
            stencilCam.projectionMatrix = cam.projectionMatrix;

            // Unsubscribe from the event after successful parameter matching
            ARSession.stateChanged -= OnStageChanged;
        }
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        effectMaterial.SetTexture("_StencilTex", stencilTex);
        Graphics.Blit(source, destination, effectMaterial);
    }
}
