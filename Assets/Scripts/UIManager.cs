﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI.Extensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using System.Linq; // TODO what is this?

public class UIManager : MonoBehaviour
{
    public GameObject objectToSpawn;
    public GameObject uiLineObject;
    private UILineRenderer uiLineRenderer;
    private ARRaycastManager rayManager;

    private bool active = true;
    private int activeCounter = 0;
    private const int COUNTER_THRESH = 2;

    private float scaleUp;
    private float offset;

    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        uiLineRenderer = uiLineObject.GetComponent<UILineRenderer>();
        rayManager = FindObjectOfType<ARRaycastManager>();
        cam = Camera.main;

        CalcScaling();
    }

    private void Update()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            PlaceARObject();
        }
    }

    // Calculate the scaling and offset that has to be put on the points
    void CalcScaling()
    {
        // NOTE: Input should be set dynamically by device
        Vector2 goal = new Vector2(Screen.height, Screen.width);
        Vector2 input = new Vector2(640, 480);

        // NOTE: The cam image was downsampled for processing by 4
        input *= 0.25f;

        // NOTE: Height is the defining factor since aspect ratios are different
        scaleUp = goal.x / input.x;

        offset = -((input.y * scaleUp) - goal.y) / 2;
    }

    public void DrawIndicator(float[] arr)
    {
        if (activeCounter < COUNTER_THRESH)
        {
            activeCounter++;
            return;
        }

        Vector2[] points = new Vector2[5];
        for (int i = 0; i < 4; i++)
        {
            Vector2 point = new Vector2( (arr[i*2] * scaleUp) + offset, arr[i*2 + 1] * scaleUp );
            points[i] = point;
        }

        points[4] = new Vector2( (arr[0] * scaleUp) + offset, arr[1] * scaleUp );
        uiLineRenderer.Points = points;

        if (!active)
        {
            uiLineObject.SetActive(true);
            active = true;

            //Debug.Log("SET_ACTIVE");
        }
    }

    public void ClearIndicator()
    {
        if (activeCounter > 0)
        {
            activeCounter--;
            return;
        }

        if (!active) return;

        uiLineObject.SetActive(false);

        active = false;

        //Debug.Log("CLEARED");
    }

    public void PlaceARObject()
    {
        if (!active) return;

        var points = uiLineRenderer.Points;

        List<Vector2> pointList = new List<Vector2>(points);

        // The last point is equal to the first point and must be removed
        pointList.RemoveAt(pointList.Count - 1);

        //foreach (Vector2 point in points)
        //{
        //    List<ARRaycastHit> hits = new List<ARRaycastHit>();
        //    rayManager.Raycast(point, hits, TrackableType.Planes);

        //    // Check hit on AR Plane
        //    if (hits.Count > 0)
        //    {
        //        Instantiate(objectToSpawn, hits[0].pose.position, hits[0].pose.rotation);
        //    }
        //    //Vector3 pos = cam.ScreenToWorldPoint(new Vector3(point.x, point.y, cam.nearClipPlane));
        //    //Instantiate(objectToSpawn, pos, Quaternion.identity);
        //}

        // Order by y so top points and bottom points can be seperated
        //pointList.Sort((a, b) => a.y.CompareTo(b.y));
        pointList = pointList.OrderBy(point => point.y).ToList();

        // Top points
        var tp1 = pointList[2];
        var tp2 = pointList[3];

        // Bottom points
        var bp1 = pointList[0];
        var bp2 = pointList[1];

        // Get distances and aspect ratio of 2D points
        float width2D = Vector2.Distance(bp1, bp2);
        float height2D = Vector2.Distance(tp1, bp1); // maybe better take top center point (perspective!)
        float ratio2D = height2D / width2D;

        // Build Vector3 Points of bottom points via raycast
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        rayManager.Raycast(bp1, hits, TrackableType.Planes);
        if (hits.Count == 0) return;
        var bp1_v3 = hits[0].pose.position;

        rayManager.Raycast(bp2, hits, TrackableType.Planes);
        if (hits.Count == 0) return;
        var bp2_v3 = hits[0].pose.position;

        // Get the point between the bottom ones
        Vector3 bottomCenter = Vector3.Lerp(bp1_v3, bp2_v3, 0.5f);

        // Calculate rotation
        Quaternion groundRotation = hits[0].pose.rotation;
        Vector3 direction = (bp1_v3 - bp2_v3).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Quaternion rotation = lookRotation * Quaternion.Euler(0, 90.0f, 0);

        // Get width and height for object
        float width = Vector3.Distance(bp1_v3, bp2_v3);
        float height = width * ratio2D;

        // Spawn object
        GameObject obj = Instantiate(objectToSpawn, bottomCenter, rotation);

        obj.transform.localScale = new Vector3(width, height, 1);
    }
}
