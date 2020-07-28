using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchManager : MonoBehaviour
{
    public GameObject touchIndicator;
    private RectTransform touchIndicatorTransform;
    private Image touchIndicatorImg;

    // Touch properties
    public float maxTouchDist = 10.0f;
    public float minTime = 0.1f;
    public float maxTime = 1.0f;

    private float startTime;
    private Vector2 startPos;

    private bool singleTouchPossible;

    public ButtonManager btnManager;
    public CameraImageManipulation cameraImageManager;

    private ScalingManager scale;

    private bool ready = false;

    IEnumerator Start()
    {
        OnboardingManager mng = FindObjectOfType<OnboardingManager>();
        scale = FindObjectOfType<ScalingManager>();

        touchIndicatorTransform = touchIndicator.GetComponent<RectTransform>();
        touchIndicatorImg = touchIndicator.GetComponent<Image>();
        touchIndicator.SetActive(false);

        while (!mng.GetComplete())
        {
            yield return null;
        }

        Debug.Log("TouchManager -> Ready");
        ready = true;
    }

    private void Update()
    {
        if (!ready) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];

            switch (touch.phase)
            {
                case TouchPhase.Began:

                    // Make sure that touch does not start on a selectable gameObject
                    // That makes touch go through text but not buttons
                    if (EventSystem.current.currentSelectedGameObject)
                    {
                        singleTouchPossible = false;
                        return;
                    }

                    startPos = touch.position;
                    startTime = Time.time;
                    break;

                case TouchPhase.Stationary:
                    singleTouchPossible = true;
                    break;

                case TouchPhase.Ended:

                    float touchTime = Time.time - startTime;

                    if (touchTime > maxTime)
                    {
                        return;
                    }

                    float distance = Vector2.Distance(touch.position, startPos);

                    if (singleTouchPossible)
                    {
                        if (touchTime < minTime)
                        {
                            Debug.Log("TOUCH too short");
                            return;
                        }

                        if (distance > maxTouchDist)
                        {
                            Debug.Log("TOUCH moved too much");
                            return;
                        }

                        Debug.Log("TOUCH for DETECTION");

                        // Display a touch point
                        touchIndicator.SetActive(true);
                        touchIndicatorTransform.position = touch.position;
                        StartCoroutine(FadeTouchOut(1.0f));

                        // Detect on camera image
                        Vector2 imgPoint = scale.PointToDetection(touch.position);
                        cameraImageManager.DetectOnImage(imgPoint);                        
                    }
                    break;
            }
        }
    }

    IEnumerator FadeTouchOut(float time)
    {
        float currentTime = 0;
        Color transparent = new Color(255.0f, 255.0f, 255.0f, 0.0f);

        do
        {
            touchIndicatorImg.color = Color.Lerp(Color.white, transparent, currentTime / time);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);

        touchIndicator.SetActive(false);
    }
}
