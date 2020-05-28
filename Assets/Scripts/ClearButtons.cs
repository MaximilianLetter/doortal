using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(TextManager))]
public class ClearButtons : MonoBehaviour
{
    private ARSession session;
    private ARPlaneManager planeManager;

    private PortalManager portalManager;
    private TextManager textManager;

    //private OnboardingManager onboardingManager;
    //private WaitForOnboardingComplete buttonDisplayment;
    //private CameraImageManipulation doorDetection;

    void Start()
    {
        session = FindObjectOfType<ARSession>();
        planeManager = FindObjectOfType<ARPlaneManager>();

        portalManager = FindObjectOfType<PortalManager>();
        textManager = GetComponent<TextManager>();
        //onboardingManager = FindObjectOfType<OnboardingManager>();
        //buttonDisplayment = FindObjectOfType<WaitForOnboardingComplete>();
        //doorDetection = FindObjectOfType<CameraImageManipulation>();
    }

    public void RemoveAugmentation()
    {
        portalManager.DestroyPortal();

        textManager.ShowNotification(TextContent.doorCleared);
    }

    public void RemovePlanes()
    {
        // Possibility 1: Reset AR Session and display message
        portalManager.DestroyPortal();

        // Destroy all currently detected planes manually, after that reset the session
        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
        session.Reset();

        // Display message
        textManager.ShowNotification(TextContent.everythingCleared);

        // Possibility 2: Reset all parts of the app
        // Delete current possible existing portals
        //portalManager.DestroyPortal();

        //// Destroy all currently detected planes manually, after that reset the session
        //foreach (var plane in planeManager.trackables)
        //{
        //    Debug.Log("plane deleted");
        //    plane.gameObject.SetActive(false);
        //}
        //session.Reset();

        //// Restart the onboarding 
        //onboardingManager.StartCoroutine("Onboarding");

        //// Disable all buttons until the onboarding is complete
        //buttonDisplayment.StartCoroutine("Start");

        //// Stop the door detection until the onboarding is complete
        //// NOTE: doorDetection Stop function was removed
        //doorDetection.Stop();
        //doorDetection.StartCoroutine("Start");

        //Debug.Log("Successful reset");
    }
}
