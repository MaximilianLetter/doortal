using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class OnboardingManager : MonoBehaviour
{
    public TextManager textManager;
    public GameObject onboardingPlanePrefab;

    public ARSession session;
    private bool complete;

    private ARPlaneManager planeManager;

    IEnumerator Start()
    {
        if ((ARSession.state == ARSessionState.None) ||
            (ARSession.state == ARSessionState.CheckingAvailability))
        {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            textManager.ShowNotification(TextContent.notSupported);
        }
        else
        {
            StartCoroutine(Onboarding());
            session.enabled = true;
        }
    }

    IEnumerator Onboarding()
    {
        complete = false;

        planeManager = FindObjectOfType<ARPlaneManager>();
        planeManager.planePrefab = onboardingPlanePrefab;

        textManager.ShowNotification(TextContent.scanGround);

        // Wait the minimum of 1 second before checking
        yield return new WaitForSeconds(1);

        // Wait until a plane is found
        while (planeManager.trackables.count == 0)
        {
            yield return null;
        }

        // Wait 1 more second before hiding the plane
        yield return new WaitForSeconds(1);

        // Start FadeOff animation
        foreach (ARPlane plane in planeManager.trackables)
        {
            GameObject obj = plane.gameObject;
            obj.GetComponent<Animator>().SetBool("FadeOff", true);
            Destroy(obj.GetComponent<FadePlaneOnBoundaryChange>());

        }
        // Make sure future planes are not having prefab properties
        planeManager.planePrefab = null;

        // The plane is now showing, wait for 2 more seconds until the animation is done
        yield return new WaitForSeconds(2);

        textManager.ShowNotification(TextContent.onboardingComplete);

        // Set onboarding complete
        complete = true;
        
        // Now that the onboarding plane is hidden, remove the other prefab properties
        foreach (ARPlane plane in planeManager.trackables)
        {
            GameObject obj = plane.gameObject;

            // Delete the preview material from the plane
            Destroy(obj.GetComponent<ARFeatheredPlaneMeshVisualizer>());
            Destroy(obj.GetComponent<Animator>());
            Destroy(obj.GetComponent<MeshRenderer>());
        }
    }

    public bool GetComplete()
    {
        return complete;
    }
}
