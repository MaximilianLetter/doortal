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
    //public ImageToWorld imgToWorld;

    private bool ready = false;

    IEnumerator Start()
    {
        OnboardingManager mng = FindObjectOfType<OnboardingManager>();

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
                    singleTouchPossible = true;

                    startPos = touch.position;
                    startTime = Time.time;
                    break;

                case TouchPhase.Stationary:
                    swipePossible = false;
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
                        Debug.Log("TOUCH for PORTAL");

                        // Version2
                        
                        portalManager.SpawnPortal();

                        // Version1
                        //if (!IsOnUI())
                        //{
                        //    portalManager.SpawnPortal();
                        //}
                    }
                    break;

            }
        }
    }

    private bool IsOnUI()
    {
        PointerEventData eventDataPos = new PointerEventData(EventSystem.current);
        eventDataPos.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataPos, results);

        return results.Count > 0;
    }

}
