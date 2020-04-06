﻿using System;
using Unity.Collections;
using UnityEngine.UI;

using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using System.Runtime.InteropServices;

// NOTE: based on documentation: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@2.1/manual/cpu-camera-image.html#synchronously-convert-to-grayscale-and-color
// Some namings seem to be different in documentation and package
public class CameraImageManipulation : MonoBehaviour
{
    // AR Foundation Setup
    private ARCameraManager cameraManager;
    private Texture2D camTexture;

    // WebCam Setup
    private WebCamTexture webCam;

    // UI to display on
    private RawImage rawImage;
    public GameObject uiDisplay;

    private UIManager uiManager;
    public GameObject ui;

    // Array to catch OpenCV results
    //private NativeArray<byte> nativeByteArray;
    private NativeArray<float> nativeByteArray;
    private float[] managedArray;

    // NOTE: UNITY_ANDROID is always active since its the build platform
#if UNITY_EDITOR
    private const string LIBRARY_NAME = "ComputerVision";
#elif UNITY_ANDROID
    private const string LIBRARY_NAME = "native-lib";
#endif

    [DllImport(LIBRARY_NAME)]
    private unsafe static extern void ProcessImage(void* result, ref Color32[] rawImage, int width, int height);

    void OnEnable()
    {
        if (rawImage == null)
        {
            rawImage = uiDisplay.GetComponent<RawImage>();
            uiManager = ui.GetComponent<UIManager>();
        }
#if UNITY_EDITOR
        Debug.Log("UNITY_EDITOR | WEBCAM_ENABLE");

        webCam = new WebCamTexture();

        //nativeByteArray = new NativeArray<byte>(8, Allocator.Persistent);
        const int CORNERS = 4;
        nativeByteArray = new NativeArray<float>(CORNERS * 2, Allocator.Persistent);
        managedArray = new float[CORNERS * 2];

        rawImage.texture = webCam;
        //rawImage.material.mainTexture = webCam;

        webCam.Play();
#elif UNITY_ANDROID
        if (cameraManager == null)
        {
            GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
            cameraManager = cam.GetComponent<ARCameraManager>();
        }

        // NOTE: documentation says 'cameraFrameReceived'
        cameraManager.frameReceived += OnCameraFrameReceived;
#endif
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        Debug.Log("UNITY_EDITOR | WEBCAM_DISABLE");
        webCam.Stop();

        nativeByteArray.Dispose();
#elif UNITY_ANDROID
        // NOTE: documentation says 'cameraFrameReceived'
        cameraManager.frameReceived -= OnCameraFrameReceived;
#endif
    }

#if UNITY_EDITOR
    void Update()
    {
        if (webCam.isPlaying)
        {
            OnWebcamFrameReceived();
        }
    }
#endif

    unsafe void OnWebcamFrameReceived()
    {
        var pixels = webCam.GetPixels32();

        // Call to C++ Code
        void* ptr = NativeArrayUnsafeUtility.GetUnsafePtr(nativeByteArray);

        ProcessImage(ptr, ref pixels, webCam.width, webCam.height);

        //for (int i = 0; i < nativeByteArray.Length; i += 2)
        //{
        //    if (i == 0) Debug.Log("______");
        //    Debug.Log(nativeByteArray[i]);
        //    Debug.Log(nativeByteArray[i+1]);
        //}

        nativeByteArray.CopyTo(managedArray);

        uiManager.DrawIndicator(managedArray);

        camTexture = new Texture2D(
            webCam.width,
            webCam.height
        );

        camTexture.SetPixels32(pixels);
        camTexture.Apply();

        rawImage.texture = camTexture;
    }

    unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        // NOTE: documentation says 'CameraImage'
        XRCameraImage image;
        if (!cameraManager.TryGetLatestImage(out image))
            return;

        // NOTE: documentation says 'CameraImageConversionParams'
        var conversionParams = new XRCameraImageConversionParams
        {
            // Get the entire image
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 4
            outputDimensions = new Vector2Int(image.width / 4, image.height / 4),

            // Choose RGBA format
            outputFormat = TextureFormat.RGBA32,
            //outputFormat = TextureFormat.R8,

            // Flip across the vertical axis (mirror image)
            //transformation = CameraImageTransformation.MirrorY
            transformation = CameraImageTransformation.None
        };

        // See how many bytes we need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

        // The image was converted to RGB8 format and written into the provided buffer
        // so we can dispose of the CameraImage. We must do this or it will leak resources.
        image.Dispose();

        // At this point, we could process the image, pass it to a computer vision algorithm, etc.
        camTexture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false
        );

        camTexture.LoadRawTextureData(buffer);
        camTexture.Apply();

        Color32[] rawPixels = camTexture.GetPixels32();
        //System.Array.Reverse(rawPixels);

        // C++ call
        Debug.Log("______PROCESS_IMAGE_________");
        //ProcessImage(ref rawPixels, conversionParams.outputDimensions.x, conversionParams.outputDimensions.y);

        camTexture.SetPixels32(rawPixels);
        camTexture.Apply();
        rawImage.texture = camTexture;

        // Done with our temporary data
        buffer.Dispose();
    }
}