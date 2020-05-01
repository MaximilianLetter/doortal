using System;
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

    private ImageToWorld imageToWorld;
    public GameObject imageToWorldObject;

    // Array to catch OpenCV results
    private const int CORNERS = 4;
    private float[] managedArray;

    // Import native C++ library
    private const string LIBRARY_NAME = "native-lib";

    [DllImport(LIBRARY_NAME)]
    private unsafe static extern bool ProcessImage(void* result, ref Color32[] rawImage, int width, int height, bool rotated);

    void OnEnable()
    {
        imageToWorld = imageToWorldObject.GetComponent<ImageToWorld>();

        // Setup result arrays
        // NativeArray  -> modified in C++
        // ManagedArray -> receives values from native
        managedArray = new float[CORNERS * 2];

        // Setup camera
        if (cameraManager == null)
        {
            GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
            cameraManager = cam.GetComponent<ARCameraManager>();
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

            // Downsample the image
            // 120p -> 0.25
            // 180p -> 0.375
            outputDimensions = new Vector2Int(Convert.ToInt32(image.width * 0.375), Convert.ToInt32(image.height * 0.375)),

            // NOTE: directly converting into single channel could be an option,
            // but it is not sure that R8 represents grayscale in one channel
            // Choose image format
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image)
            transformation = CameraImageTransformation.MirrorY
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

        Color32[] rawPixels = camTexture.GetPixels32();

        // Get pointer to nativeArray

        NativeArray<float> nativeFloatArray = new NativeArray<float>(CORNERS * 2, Allocator.Temp);
        void* ptr = NativeArrayUnsafeUtility.GetUnsafePtr(nativeFloatArray);

        // Call to C++ Code
        bool success = ProcessImage(ptr, ref rawPixels, conversionParams.outputDimensions.x, conversionParams.outputDimensions.y, true);
        
        if (success)
        {
            nativeFloatArray.CopyTo(managedArray);

            imageToWorld.ShowIndicator(true, managedArray);
        }
        else
        {
            imageToWorld.ShowIndicator(false, null);
        }

        // Done with our temporary data
        buffer.Dispose();
        nativeFloatArray.Dispose();
    }
}
