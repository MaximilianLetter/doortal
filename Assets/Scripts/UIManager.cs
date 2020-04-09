using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI.Extensions;

public class UIManager : MonoBehaviour
{
    public GameObject uiLineObject;
    private UILineRenderer uiLineRenderer;

    private bool active = true;

    private float scaleUp;
    private float offset;

    // Start is called before the first frame update
    void Start()
    {
        uiLineRenderer = uiLineObject.GetComponent<UILineRenderer>();

        CalcScaling();
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
        if (!active) return;

        //Vector2[] clearArray = new Vector2[4];
        //uiLineRenderer.Points = clearArray;

        //uiLineRenderer.SetAllDirty();

        uiLineObject.SetActive(false);

        active = false;

        //Debug.Log("CLEARED");
    }
}
