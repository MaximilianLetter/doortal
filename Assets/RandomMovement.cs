using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MoveInBounds());
    }

    IEnumerator MoveInBounds()
    {
        while(true)
        {
            transform.localPosition = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
            yield return new WaitForSeconds(Random.Range(0.5f, 3.0f));
        }
    }
}
