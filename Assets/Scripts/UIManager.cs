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

    // Start is called before the first frame update
    void Start()
    {
        uiLineRenderer = uiLineObject.GetComponent<UILineRenderer>();
    }

    public void DrawIndicator(float[] arr)
    {
        int scaleUp = 1;
        for (int i = 0; i < 4; i++)
        {
            Vector2 point = new Vector2( arr[i*2] * scaleUp, arr[i*2 + 1] * scaleUp );
            uiLineRenderer.Points.SetValue(point, i);
        }
        uiLineRenderer.SetAllDirty();

        if (!active)
        {
            uiLineObject.SetActive(true);
            active = true;

            Debug.Log("SET_ACTIVE");
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

        Debug.Log("CLEARED");
    }
}
