﻿using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine.UI;

// NOTE: based on documentation: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@2.1/manual/cpu-camera-image.html#synchronously-convert-to-grayscale-and-color
// Some namings seem to be different in documentation and package
public class CameraImageManipulation : MonoBehaviour
{
    // AR Foundation Setup
    private ARCameraManager cameraManager;
    private Texture2D camTexture;

    // Door detection result consists of 4 corners
    private const int CORNERS = 4;

    // Script that holds the next step in processing the image
    private ScalingManager scale;
    private ImageToWorld imageToWorld;

    // Import function from native C++ library
    private const string LIBRARY_NAME = "native-lib";

    [DllImport(LIBRARY_NAME)]
    private unsafe static extern bool ProcessImage(
        Vector2* resultArray,
        Color32[] rawImage,
        Vector2 userInput,
        int width,
        int height,
        bool rotated
    );

    // NOTE: capsulating the extern function for manipulating the result array
    // https://stackoverflow.com/questions/53174216/update-vector3-array-from-c-native-plugin
    bool ProcessImage(Vector2[] vecArray, Color32[] rawImage, Vector2 userInput, int width, int height, bool rotated)
    {
        unsafe
        {
            // Pin array then send to C++
            fixed (Vector2* vecPtr = vecArray)
            {
                return ProcessImage(vecPtr, rawImage, userInput, width, height, rotated);
            }
        }
    }


    IEnumerator Start()
    {
        OnboardingManager mng = FindObjectOfType<OnboardingManager>();

        scale = FindObjectOfType<ScalingManager>();
        imageToWorld = FindObjectOfType<ImageToWorld>();

        while (!mng.GetComplete())
        {
            yield return null;
        }

        Debug.Log("CameraImageManipulation -> Ready");
        cameraManager = Camera.main.GetComponent<ARCameraManager>();
    }

    // NOTE: part of the following steps to setup image access on CPU come from
    // the official AR Foundation documentation. However some functions use
    // other names than described in the documentation.
    // e.g. cameraManager.frameReceived instead of cameraManager.cameraFrameReceived
    // https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@3.1/manual/cpu-camera-image.html
    public unsafe void DetectOnImage(Vector2 userInput)
    {
        XRCameraImage image;
        if (!cameraManager.TryGetLatestImage(out image))
            return;

        var conversionParams = new XRCameraImageConversionParams
        {
            // Get the entire image
            inputRect = new RectInt(0, 0, image.width, image.height),

            outputDimensions = new Vector2Int(Convert.ToInt32(image.width * scale.detectionScaleFactor), Convert.ToInt32(image.height * scale.detectionScaleFactor)),

            // NOTE: directly converting into single channel could be an option,
            // but it is not sure that R8 represents grayscale in one channel
            // NOTE 2: RGBA32 is not listed in the documentation as supported format
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image)
            transformation = CameraImageTransformation.None
        };

        // See how many bytes we need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

        // The image was converted to RGB32 format and written into the provided buffer
        // so we can dispose the CameraImage. We must do this or it will leak resources.
        image.Dispose();

        // At this point, we could process the image, pass it to a computer vision algorithm, etc.
        if (camTexture == null)
        {
            camTexture = new Texture2D(
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                conversionParams.outputFormat,
                false
            );
        }

        camTexture.LoadRawTextureData(buffer);
        camTexture.Apply();

        Color32[] rawPixels = camTexture.GetPixels32();
        Vector2[] resultArray = new Vector2[CORNERS];

        // Call to C++ Code
        float startT = Time.realtimeSinceStartup;
        bool success = ProcessImage(resultArray, rawPixels, userInput, conversionParams.outputDimensions.x, conversionParams.outputDimensions.y, true);
        float endT = Time.realtimeSinceStartup;

        Debug.Log("DetectionTime: ");
        Debug.Log(endT - startT);

        imageToWorld.TransferIntoWorld(success, resultArray);
        //imageToWorld.ShowIndicator(success, resultArray);

        // Done with our temporary data
        buffer.Dispose();
    }
}
