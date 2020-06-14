using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchManager : MonoBehaviour
{
    public float minSwipeDist = 25.0f;
    public float maxTime = 2.0f;

    private float startTime;
    private Vector2 startPos;

    private bool swipePossible;
    private bool singleTouchPossible;

    public ButtonManager btnManager;
    public PortalManager portalManager;
    public CameraImageManipulation cameraImageManager;
    //public ImageToWorld imgToWorld;

    private bool ready = false;

    private Vector2 camImageSize = new Vector2(640, 480);
    private float scaleDown;
    private Vector2 offset;

    IEnumerator Start()
    {
        OnboardingManager mng = FindObjectOfType<OnboardingManager>();

        CalcScaling();

        while (!mng.GetComplete())
        {
            yield return null;
        }

        Debug.Log("TouchManager -> Ready");
        ready = true;
    }

    private void CalcScaling()
    {
        Vector2 screen = new Vector2(Screen.height, Screen.width);
        Vector2 goal = camImageSize;

        // NOTE: Aspect ratios are different, therefor an offset needs to be calculated
        scaleDown = goal.x / screen.x;

        offset = new Vector2(-((screen.y * scaleDown) - goal.y) / 2, 0);
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

                        Vector2 imgPoint = (touch.position * scaleDown) + offset;

                        // In OpenCV, y 0 starts top, Unity starts bottom
                        imgPoint = new Vector2(imgPoint.x, camImageSize.x - imgPoint.y);
                        Debug.Log(touch.position);
                        Debug.Log(imgPoint);
                        cameraImageManager.DetectOnImage(imgPoint);
                        
                    }
                    break;

            }
        }
    }
}
