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

    // WebCam Setup
    private WebCamTexture webCam;

    // UI to display on
    private RawImage rawImage;
    public GameObject uiDisplay;

    private ImageToWorld imageToWorld;
    public GameObject imageToWorldObject;

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
    private unsafe static extern bool ProcessImage(void* result, ref Color32[] rawImage, int width, int height, bool rotated);

    void OnEnable()
    {
        if (rawImage == null)
        {
            rawImage = uiDisplay.GetComponent<RawImage>();
            imageToWorld = imageToWorldObject.GetComponent<ImageToWorld>();
        }

        //nativeByteArray = new NativeArray<byte>(8, Allocator.Persistent);
        const int CORNERS = 4;
        nativeByteArray = new NativeArray<float>(CORNERS * 2, Allocator.Persistent);
        managedArray = new float[CORNERS * 2];

#if UNITY_EDITOR
        Debug.Log("UNITY_EDITOR | WEBCAM_ENABLE");

        webCam = new WebCamTexture();

        rawImage.texture = webCam;
        //rawImage.material.mainTexture = webCam;

        webCam.Play();
        Vector2 size = new Vector2(webCam.width, webCam.height);
        uiDisplay.GetComponent<RectTransform>().sizeDelta = size;

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
        nativeByteArray.Dispose();
#if UNITY_EDITOR
        Debug.Log("UNITY_EDITOR | WEBCAM_DISABLE");
        webCam.Stop();

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

        // if a rectangle was found
        if (ProcessImage(ptr, ref pixels, webCam.width, webCam.height, false))
        {
            nativeByteArray.CopyTo(managedArray);

            imageToWorld.DrawIndicator(managedArray);
        } else
        {
            imageToWorld.ClearIndicator();
        }

        //for (int i = 0; i < nativeByteArray.Length; i += 2)
        //{
        //    if (i == 0) Debug.Log("______");
        //    Debug.Log(nativeByteArray[i]);
        //    Debug.Log(nativeByteArray[i+1]);
        //}

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

        //Debug.Log(image.width);
        //Debug.Log(image.height);

        // NOTE: documentation says 'CameraImageConversionParams'
        var conversionParams = new XRCameraImageConversionParams
        {
            // Get the entire image
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 4
            // 120p -> 0.25
            // 180p -> 0.375
            outputDimensions = new Vector2Int(Convert.ToInt32(image.width * 0.375), Convert.ToInt32(image.height * 0.375)),

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
        //camTexture.Apply();

        Color32[] rawPixels = camTexture.GetPixels32();
        //System.Array.Reverse(rawPixels);

        // Call to C++ Code
        void* ptr = NativeArrayUnsafeUtility.GetUnsafePtr(nativeByteArray);

        // if a rectangle was found
        if (ProcessImage(ptr, ref rawPixels, conversionParams.outputDimensions.x, conversionParams.outputDimensions.y, true))
        {
            nativeByteArray.CopyTo(managedArray);

            imageToWorld.DrawIndicator(managedArray);
        }
        else
        {
            imageToWorld.ClearIndicator();
        }

        // Done with our temporary data
        buffer.Dispose();
    }
}
