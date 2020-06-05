using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI.Extensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using System.Linq; // TODO what is this?
using UnityEngine.UI;

public class ImageToWorld : MonoBehaviour
{
    public PortalManager portalManager;
    public TextManager textManager;
    //public GameObject helperQuad;

    public GameObject spawnHelper;
    public GameObject doorIndicator;
    private UILineRenderer uiLineRenderer;
    private RectTransform doorButton;
    private ARRaycastManager rayManager;
    private Camera cam;

    // Amount of time a marker is hold before the marker disappears
    public float holdTime;
    private float foundTime = 0.0f;
    private bool foundStatus = false;

    // These values are not used but could be if a stricter smoothing algorithm is implemented
    //private int resolution = Screen.width; // END resolution
    //private float DIFF_THRESH = Screen.width / 20;
    //private float DIFF_THRESH_CANCEL = DIFF_THRESH * 2;
    //private const int COUNTER_THRESH = 2;
    //private List<Vector2> previousDoor = new List<Vector2>();
    //private int prevCounter = 0;

    private bool readyToPlace = false;

    private Vector2 imgInputSize = new Vector2(640, 480);
    private float scaleUp;
    private Vector2 offset;

    void Start()
    {
        uiLineRenderer = doorIndicator.transform.Find("UI LineRenderer").GetComponent<UILineRenderer>();
        doorButton = doorIndicator.transform.Find("DoorButton").GetComponent<RectTransform>();
        rayManager = FindObjectOfType<ARRaycastManager>();
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        //helperQuad.SetActive(false);

        CalcScaling();
    }

    // Calculate the scaling and offset that has to be put on the points
    void CalcScaling()
    {
        // NOTE: Input should be set dynamically by device
        Vector2 goal = new Vector2(Screen.height, Screen.width);

        // NOTE: The cam image was downsampled for processing
        // 120p -> 0.25f
        // 180p -> 0.375f
        Vector2 inputDownscaled = imgInputSize * 0.375f;

        // NOTE: Aspect ratios are different, there for an offset needs to be calculated
        scaleUp = goal.x / inputDownscaled.x;

        offset = new Vector2(-((inputDownscaled.y * scaleUp) - goal.y) / 2, 0);
    }

    public void ShowIndicator(bool foundNew, Vector2[] arr)
    {
        if (foundNew)
        {
            // Check if last frame something was found to avoid 1 frame flickerings
            if (!foundStatus)
            {
                foundStatus = true;
                foundTime = 0.0f;
                return;
            }

            foundTime = 0.0f;

            // Start values for setting up button and icon on the detected door
            Vector2 centroid = Vector2.zero;
            float maxX = 0;
            float minX = Screen.width;
            float maxY = 0;
            float minY = Screen.height;

            // Scale and offset the result array
            List<Vector2> door = new List<Vector2>();
            for (int i = 0; i < 4; i++)
            {
                //Vector2 point = new Vector2((arr[i * 2] * scaleUp) + offset, arr[i * 2 + 1] * scaleUp);
                Vector2 point = (arr[i] * scaleUp) + offset;
                door.Add(point);

                centroid += point;
                if (point.x > maxX) maxX = point.x;
                if (point.x < minX) minX = point.x;
                if (point.y > maxY) maxY = point.y;
                if (point.y < minY) minY = point.y;
            }
            centroid *= 0.25f;
            door.Add((arr[0] * scaleUp) + offset);

            uiLineRenderer.Points = door.ToArray();

            // Set up button and icon
            doorButton.position = centroid;
            doorButton.sizeDelta = new Vector2(maxX - minX, maxY - minY);

            // If inactive, activate the line renderer and prepare to place an object
            if (!doorIndicator.activeSelf)
            {
                doorIndicator.SetActive(true);
                readyToPlace = true;
            }
        }
        else
        {
            foundTime += Time.deltaTime;

            if (foundTime > holdTime)
            {
                // If no new positive input appears, reset everything
                foundStatus = false;

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
    //private bool CheckDifferences(List<Vector2> newDoor)
    //{
    //    int diffCounter = 0;

    //    if (previousDoor.Count == 0)
    //    {
    //        return false;
    //    }

    //    for (int i = 0; i < 4; i++)
    //    {
    //        float difference = Vector2.Distance(previousDoor[i], newDoor[i]);

    //        if (difference > DIFF_THRESH)
    //        {
    //            diffCounter += 1;

    //            if (difference > DIFF_THRESH_CANCEL)
    //            {
    //                return false;
    //            }
    //        }
    //    }

    //    if (diffCounter > 2)
    //    {
    //        prevCounter += 1;

    //        if (prevCounter > COUNTER_THRESH)
    //        {
    //            previousDoor = new List<Vector2>();
    //            prevCounter = 0;
    //        }

    //        return false;
    //    }

    //    return true;
    //}

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

        // Build Vector3 Points of bottom points via raycast
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        rayManager.Raycast(bp1, hits, TrackableType.Planes);
        if (hits.Count == 0)
        {
            textManager.ShowNotification(TextContent.noGround);
            return;
        }
        var bp1_v3 = hits[0].pose.position;
        Debug.Log("Ground Rotation1 " + hits[hits.Count - 1].pose.rotation.eulerAngles);

        rayManager.Raycast(bp2, hits, TrackableType.Planes);
        if (hits.Count == 0)
        {
            textManager.ShowNotification(TextContent.noGround);
            return;
        }
        var bp2_v3 = hits[0].pose.position;
        Debug.Log("Ground Rotation2 " + hits[hits.Count - 1].pose.rotation.eulerAngles);

        // Unify the height of both points
        float unifyY = (bp1_v3.y + bp2_v3.y) / 2;
        bp1_v3.y = unifyY;
        bp2_v3.y = unifyY;

        // Get the center between the bottom points
        Vector3 bottomCenter = Vector3.Lerp(bp1_v3, bp2_v3, 0.5f);
        Debug.Log("ImageToWorld, bottomCenter: " + bottomCenter);

        // Calculate rotation
        Vector3 direction = (bp1_v3 - bp2_v3).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Quaternion rotation = lookRotation * Quaternion.Euler(0, 90.0f, 0);

        // Check if there are really points behind the detected rectangle to verify it is a door
        // NOTE: First crappy version
        ARPointCloud cloud = FindObjectOfType<ARPointCloud>();
        Vector3 camPos = Camera.main.transform.position;
        float distToDoor = Vector3.Distance(camPos, bottomCenter);
        float minOffset = 0.5f;
        bool spaceBehindDoor = false;
        int count = 0;
        foreach (Vector3 point in cloud.positions)
        {
            count++;
            if (Vector3.Distance(camPos, point) > distToDoor + minOffset)
            {
                Debug.Log("THERE IS A FURTHER POINT");
                spaceBehindDoor = true;
                break;
            }
        }
        if (!spaceBehindDoor)
        {
            textManager.ShowNotification(TextContent.noRealDoor);
            return;
        }
        Debug.Log("POINTS: " + count);

        // Get width for object
        float width = Vector3.Distance(bp1_v3, bp2_v3);

        // Calulate height with help of a vertical quad that can be raycasted against
        GameObject quad = Instantiate(spawnHelper, bottomCenter, rotation);
        RaycastHit hit;
        Vector3 tp1_v3, tp2_v3;
        Ray ray;

        ray = cam.ScreenPointToRay(tp1);
        Physics.Raycast(ray, out hit);
        Debug.Log("top raycast1: " + hit.point);
        tp1_v3 = hit.point;

        ray = cam.ScreenPointToRay(tp2);
        Physics.Raycast(ray, out hit);
        Debug.Log("top raycast2: " + hit.point);
        tp2_v3 = hit.point;

        Vector3 topCenter = Vector3.zero;

        // Raycast did not hit the correct plane and is therefor not usable
        if (tp1_v3 == Vector3.zero || tp2_v3 == Vector3.zero)
        {
            Debug.Log("RAYCAST DID NOT HIT");
        }
        else
        {
            topCenter = Vector3.Lerp(tp1_v3, tp2_v3, 0.5f);
        }
        Debug.Log("ImageToWorld, topCenter: " + topCenter);


        //Vector3 botToTopdir = (tp1_v3 - tp2_v3).normalized;
        //Quaternion botToTopRot = Quaternion.LookRotation(direction);
        //Debug.Log("Bottom to Top Rot " + botToTopRot.eulerAngles);

        //float angleY = botToTopRot.eulerAngles.y;
        //float angleDiffY = angleY > 180 ? 360 - angleY : angleY;

        //rotation = Quaternion.Euler(rotation.eulerAngles + new Vector3(0, angleDiffY, 0));
        //Debug.Log("rotation afterwards" + rotation.eulerAngles);
        // TODO -> rotation von boden zu oben ist meist 340° oder ähnliches -> differenz zu 0 / 360 sollte als rotation aufgenommen werden

        float height = Vector3.Distance(bottomCenter, topCenter);
        Debug.Log("ImageToWorld, height: " + height);

        // Destroy the used quad
        Destroy(quad);
        //helperQuad.SetActive(false);

        // Spawn object
        portalManager.SpawnObject(bottomCenter, rotation, width, height);
    }
}
