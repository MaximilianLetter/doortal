using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI.Extensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using System.Linq; // TODO what is this?

public class ImageToWorld : MonoBehaviour
{
    public PortalManager portalManager;

    public GameObject objectToSpawn;
    public GameObject doorIndicator;
    private UILineRenderer uiLineRenderer;
    private RectTransform doorButton;
    private ARRaycastManager rayManager;

    // These values are not used but could be if a stricter smoothing algorithm is implemented
    private const int resolution = 1080; // END resolution
    private const float DIFF_THRESH = resolution / 20;
    private const float DIFF_THRESH_CANCEL = DIFF_THRESH * 2;
    private const int COUNTER_THRESH = 2;
    private List<Vector2> previousDoor = new List<Vector2>();
    private int prevCounter = 0;

    private bool readyToPlace = false;
    private bool showLines = false;
    private bool holdLines = false;

    private float scaleUp;
    private float offset;

    void Start()
    {
        uiLineRenderer = doorIndicator.transform.Find("UI LineRenderer").GetComponent<UILineRenderer>();
        doorButton = doorIndicator.transform.Find("DoorButton").GetComponent<RectTransform>();
        rayManager = FindObjectOfType<ARRaycastManager>();

        CalcScaling();
    }

    // Calculate the scaling and offset that has to be put on the points
    void CalcScaling()
    {
        // NOTE: Input should be set dynamically by device
        Vector2 goal = new Vector2(Screen.height, Screen.width);
        Vector2 input = new Vector2(640, 480);

        // NOTE: The cam image was downsampled for processing
        // 120p -> 0.25f
        // 180p -> 0.375f
        input *= 0.375f;

        // NOTE: Height is the defining factor since aspect ratios are different
        scaleUp = goal.x / input.x;

        offset = -((input.y * scaleUp) - goal.y) / 2;
    }

    public void ShowIndicator(bool foundNew, float[] arr)
    {
        if (foundNew)
        {
            // To reduce flickering, dismiss the first result
            if (!showLines)
            {
                showLines = true;
                holdLines = true;
            }
            else
            {
                // Start values for setting up button and icon on the detected door
                Vector2 centroid = Vector2.zero;
                float maxX = 0;
                float minX = 1080.0f;
                float maxY = 0;
                float minY = 1920.0f;

                // Convert the result array to a Vector2 List
                List<Vector2> door = new List<Vector2>();
                for (int i = 0; i < 4; i++)
                {
                    Vector2 point = new Vector2((arr[i * 2] * scaleUp) + offset, arr[i * 2 + 1] * scaleUp);
                    door.Add(point);

                    centroid += point;
                    if (point.x > maxX) maxX = point.x;
                    if (point.x < minX) minX = point.x;
                    if (point.y > maxY) maxY = point.y;
                    if (point.y < minY) minY = point.y;
                }
                door.Add(new Vector2((arr[0] * scaleUp) + offset, arr[1] * scaleUp));

                uiLineRenderer.Points = door.ToArray();

                // Set up button and icon
                doorButton.position = centroid * 0.25f;
                doorButton.sizeDelta = new Vector2(maxX - minX, maxY - minY);

                // If inactive, activate the line renderer and prepare to place an object
                if (!doorIndicator.activeSelf)
                {
                    doorIndicator.SetActive(true);
                    readyToPlace = true;
                }
            }
        }
        else
        {
            // If the previous input was positive hold it for a frame
            if (holdLines)
            {
                holdLines = false;
            }
            else
            {
                // If no new positive input appears, reset everything
                showLines = false;

                if (doorIndicator.activeSelf)
                {
                    doorIndicator.SetActive(false);
                    readyToPlace = false;
                }
            }
        }
    }

    // NOTE: this function is not used but can be for a better smoothing
    // Test how many differences a proposed door has to the previous door
    private bool CheckDifferences(List<Vector2> newDoor)
    {
        int diffCounter = 0;

        if (previousDoor.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < 4; i++)
        {
            float difference = Vector2.Distance(previousDoor[i], newDoor[i]);

            if (difference > DIFF_THRESH)
            {
                diffCounter += 1;

                if (difference > DIFF_THRESH_CANCEL)
                {
                    return false;
                }
            }
        }

        if (diffCounter > 2)
        {
            prevCounter += 1;

            if (prevCounter > COUNTER_THRESH)
            {
                previousDoor = new List<Vector2>();
                prevCounter = 0;
            }

            return false;
        }

        return true;
    }

    // Place the object based on the current proposed door
    public void PlaceObject()
    {
        if (!readyToPlace) return;

        var points = uiLineRenderer.Points;

        List<Vector2> pointList = new List<Vector2>(points);

        // The last point is equal to the first point and must be removed
        pointList.RemoveAt(pointList.Count - 1);

        // Order by y so top points and bottom points can be seperated
        // pointList.Sort((a, b) => a.y.CompareTo(b.y));
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

        // Get the center between the bottom points
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
        portalManager.SpawnObject(bottomCenter, rotation, width, height);
    }
}
