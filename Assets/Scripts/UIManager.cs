using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI.Extensions;

public class UIManager : MonoBehaviour
{
    private UILineRenderer uiLineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        uiLineRenderer = transform.Find("UI LineRenderer").GetComponent<UILineRenderer>();
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
    }
}
