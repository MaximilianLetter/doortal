using System;
using Unity.Collections;
using UnityEngine.UI;

using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// NOTE: based on documentation: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@2.1/manual/cpu-camera-image.html#synchronously-convert-to-grayscale-and-color
// Some namings seem to be different in documentation and package
public class DoorDetection : MonoBehaviour
{
    private ARCameraManager cameraManager;
    private Texture2D m_Texture;
    private RawImage rawImage;

    public GameObject uiDisplay;

    void OnEnable()
    {
        if (cameraManager == null)
        {
            GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
            cameraManager = cam.GetComponent<ARCameraManager>();
        }
        if (rawImage == null)
        {
            rawImage = uiDisplay.GetComponent<RawImage>();
        }

        // NOTE: documentation says 'cameraFrameReceived'
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        // NOTE: documentation says 'cameraFrameReceived'
        cameraManager.frameReceived -= OnCameraFrameReceived;
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
            outputFormat = TextureFormat.R8,

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
        m_Texture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false
        );

        m_Texture.LoadRawTextureData(buffer);
        m_Texture.Apply();

        rawImage.texture = m_Texture;

        // Done with our temporary data
        buffer.Dispose();
    }
}
