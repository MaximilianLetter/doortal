using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalingManager : MonoBehaviour
{
    // 120p -> 0.25f -> (120, 160)
    // 180p -> 0.375f -> (180, 240)
    // 360p -> 0.75f -> (360, 480)
    // 480p -> 1.0f -> (480, 640)
    public float detectionScaleFactor = 0.75f;

    private Vector2 screenSize;
    private Vector2 arImageSize = new Vector2(480, 640);
    private Vector2 detectionImageSize;

    private float scaleUp;
    private float scaleDown;

    private Vector2 offsetUp;
    private Vector2 offsetDown;

    void Start()
    {
        screenSize = new Vector2(Screen.width, Screen.height);

        detectionImageSize = arImageSize * detectionScaleFactor;

        scaleUp = screenSize.y / detectionImageSize.y;
        offsetUp = new Vector2(-((detectionImageSize.x * scaleUp) - screenSize.x) / 2, 0);

        scaleDown = detectionImageSize.y / screenSize.y;
        offsetDown = new Vector2(-((screenSize.x * scaleDown) - detectionImageSize.x) / 2, 0);
    }

    public Vector2 PointToScreen(Vector2 point)
    {
        Vector2 newPoint;

        Debug.Log("PointToSceen---");
        Debug.Log("IN: " + point);
        //newPoint = new Vector2(point.x, detectionImageSize.y - point.y);
        //Debug.Log("IN INV: " + newPoint);

        newPoint = (point * scaleUp) + offsetUp;
        Debug.Log("OUT: " + newPoint);

        return newPoint;
    }

    public Vector2 PointToDetection(Vector2 point)
    {
        Debug.Log("PointToDetection---");
        Debug.Log("IN: " + point);
        Vector2 newPoint = (point * scaleDown) + offsetDown;
        Debug.Log("OUT: " + newPoint);

        // Invert y value of point, OpenCV 0 is top, Unity 0 is bottom
        newPoint = new Vector2(newPoint.x, detectionImageSize.y - newPoint.y);
        Debug.Log("OUT INV: " + newPoint);

        return newPoint;
    }
}
