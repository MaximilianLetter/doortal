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

    public float minSwipeDist = 25.0f;
    public float maxTime = 2.0f;

    private float startTime;
    private Vector2 startPos;

    private bool swipePossible;
    private bool singleTouchPossible;

    public ButtonManager btnManager;
    public PortalManager portalManager;
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

                    // Make sure that touch does not start on game object
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        singleTouchPossible = false;
                        swipePossible = false;
                        return;
                    }

                    swipePossible = true;

                    startPos = touch.position;
                    startTime = Time.time;
                    break;

                case TouchPhase.Stationary:
                    swipePossible = false;
                    singleTouchPossible = true;
                    break;

                case TouchPhase.Ended:

                    float touchTime = Time.time - startTime;

                    if (touchTime > maxTime)
                    {
                        return;
                    }

                    float swipeDist = Mathf.Abs(touch.position.x - startPos.x);

                    if (swipePossible && (swipeDist > minSwipeDist))
                    {
                        int swipeDirection = (int)Mathf.Sign(touch.position.y - startPos.y);
                        Debug.Log("swipe");
                        Debug.Log(swipeDirection);

                        btnManager.SwipeAugmentation(swipeDirection);
                    }
                    else if (singleTouchPossible)
                    {
                        Debug.Log("TOUCH for DETECTION");

                        touchIndicator.SetActive(true);
                        touchIndicatorTransform.position = touch.position;

                        StartCoroutine(FadeTouchOut(1.0f));


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
