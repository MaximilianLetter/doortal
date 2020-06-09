using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    public float minSwipeDist = 25.0f;

    private float startTime;
    private Vector2 startPos;

    private bool swipePossible;
    private bool singleTouchPossible;

    public ButtonManager btnManager;
    public PortalManager portalManager;
    //public ImageToWorld imgToWorld;

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    swipePossible = true;
                    startPos = touch.position;
                    startTime = Time.time;
                    break;

                case TouchPhase.Stationary:
                    swipePossible = false;
                    singleTouchPossible = true;
                    break;

                case TouchPhase.Ended:
                    float swipeTime = Time.time - startTime;
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
                        Debug.Log("TOUCH for PORTAL");
                        portalManager.SpawnPortal();
                    }
                    break;

            }
        }
    }

}
