using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(0.0001f, 1.0f)]
    public float speed = 0.1f;

    void Update()
    {
        Vector3 velocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        transform.Translate(velocity * speed);

        float rot = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            rot -= 1;
        }
        if (Input.GetKey(KeyCode.E))
        {
            rot += 1;
        }

        transform.Rotate(0, rot, 0);
    }
}
