using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowFPS : MonoBehaviour
{
    private Text text;
    float deltaTime = 0.0f;
    int frames = 0;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        frames++;

        deltaTime += Time.deltaTime;
        if (deltaTime >= 1.0f)
        {
            text.text = frames.ToString();

            deltaTime = 0.0f;
            frames = 0;
        }
    }
}
