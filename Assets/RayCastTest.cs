using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastTest : MonoBehaviour
{
    public GameObject raycastTarget;
    public GameObject testObj;
    public Camera cam;
    public Transform bottomTarget;
    public Vector2 uiPosition;

    private void Update()
    {
        Ray ray = cam.ScreenPointToRay(uiPosition);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
    }

    public void TestScenario()
    {
        // Calulate height with help of a orthogonal quad
        GameObject quad = Instantiate(raycastTarget, bottomTarget.position, bottomTarget.rotation);

        RaycastHit hit;
        Vector3 world_coords;
        Ray ray;

        ray = cam.ScreenPointToRay(uiPosition);
        Debug.Log("RAY");
        Debug.Log(ray);

        int layerMask = 1 << 9;

        bool result = Physics.Raycast(ray, out hit, layerMask);
        Debug.Log("HIT");
        Debug.Log(hit.point);

        if (!result)
        {
            Debug.Log("BUGGY");
            Destroy(quad);
            return;
        }
        world_coords = hit.transform.position;
        Debug.Log("WORLD COORDS");
        Debug.Log(world_coords);

        Instantiate(testObj, hit.point, raycastTarget.transform.rotation);

        // Destroy the used quad
        Destroy(quad);
    }
}

