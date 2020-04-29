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
    public float spawnTime;

    public GameObject objectToSpawn;
    public GameObject uiLineObject;
    private UILineRenderer uiLineRenderer;
    private ARRaycastManager rayManager;


    private const int resolution = 1080; // END resolution
    private const float DIFF_THRESH = resolution / 20;
    private const float DIFF_THRESH_CANCEL = DIFF_THRESH * 2;

    private bool readyToPlace = false;
    private const int COUNTER_THRESH = 2;
    private List<Vector2> previousDoor = new List<Vector2>();
    private int prevCounter = 0;
    private bool showLines = false;

    private float scaleUp;
    private float offset;

    // Start is called before the first frame update
    void Start()
    {
        uiLineRenderer = uiLineObject.GetComponent<UILineRenderer>();
        rayManager = FindObjectOfType<ARRaycastManager>();

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
            if (!showLines)
            {
                showLines = true;
            }
            else
            {
                List<Vector2> door = new List<Vector2>();
                for (int i = 0; i < 4; i++)
                {
                    Vector2 point = new Vector2((arr[i * 2] * scaleUp) + offset, arr[i * 2 + 1] * scaleUp);
                    door.Add(point);
                }
                door.Add(new Vector2((arr[0] * scaleUp) + offset, arr[1] * scaleUp));

                uiLineRenderer.Points = door.ToArray();

                if (!uiLineObject.activeSelf)
                {
                    uiLineObject.SetActive(true);
                    readyToPlace = true;
                }
            }
            //// Convert newly found point values to a list of 2D points
            //List<Vector2> door = new List<Vector2>();
            //for (int i = 0; i < 4; i++)
            //{
            //    Vector2 point = new Vector2((arr[i * 2] * scaleUp) + offset, arr[i * 2 + 1] * scaleUp);
            //    door.Add(point);
            //}
            //door.Add(new Vector2((arr[0] * scaleUp) + offset, arr[1] * scaleUp));

            //// Check if the found door is similar to the previous found door,
            //// if it is, replace the old one and draw the new
            //bool match = CheckDifferences(door);
            //if (match)
            //{
            //    uiLineRenderer.Points = door.ToArray();
            //    previousDoor = door;
            //    prevCounter = 0;

            //    readyToPlace = true;
            //    uiLineObject.SetActive(true);
            //}
            //else
            //{
            //    if (previousDoor.Count > 0)
            //    {
            //        // If it is not, draw the old door until it is drawn too often
            //        uiLineRenderer.Points = previousDoor.ToArray();

            //        prevCounter += 1;
            //        if (prevCounter > 2)
            //        {
            //            previousDoor = new List<Vector2>();
            //            prevCounter = 0;

            //            readyToPlace = false;
            //            uiLineObject.SetActive(false);
            //        }
            //    }
            //    else
            //    {
            //        previousDoor = door;
            //        prevCounter = 0;
            //    }
            //}
        }
        else
        {
            if (showLines)
            {
                showLines = false;
            }
            else
            {
                if (uiLineObject.activeSelf)
                {
                    uiLineObject.SetActive(false);
                    readyToPlace = true;
                }
            }
            //// No door was found but the previous could still be
            //// a good result to show
            //if (previousDoor.Count > 0)
            //{
            //    uiLineRenderer.Points = previousDoor.ToArray();

            //    // If the previous door was shown too many frames without new find,
            //    // abort it and reset to empty List
            //    prevCounter += 1;
            //    if (prevCounter > 2)
            //    {
            //        previousDoor = new List<Vector2>();
            //        prevCounter = 0;

            //        readyToPlace = false;
            //        uiLineObject.SetActive(false);
            //    }
            //}
        }
    }

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

    public void PlaceARObject()
    {
        if (!readyToPlace) return;

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

        obj.transform.localScale = new Vector3(0.01f, 0.01f, 1);

        StartCoroutine(ScaleOverTime(obj, spawnTime, width, height));
    }

    private IEnumerator ScaleOverTime(GameObject obj, float time, float width, float height)
    {
        Vector3 originalScale = obj.transform.localScale;
        Vector3 destinationScale = new Vector3(width, height, 1.0f);

        float currentTime = 0.0f;

        do
        {
            obj.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / time);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);

        // Make sure the endresult is the destination
        obj.transform.localScale = destinationScale;
    }
}
