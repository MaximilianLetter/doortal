using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishLogic : MonoBehaviour
{
    public float avgSpeed = 0.3f;
    public float catchUpSpeed = 0.5f; 
    public float rotationSpeed = 3.0f;
    public float neighbourDistance = 0.5f;
    public float avoidanceDistance = 0.1f;

    [HideInInspector]
    public float speed;

    private FishSwarm fishManager;
    private bool turning;

    void Start()
    {
        speed = Random.Range(avgSpeed * 0.75f, avgSpeed * 1.25f);

        GameObject obj = GameObject.Find("FishManager");
        fishManager = obj.GetComponent<FishSwarm>();

        // TODO: Check if a smarter way of updating the logic and catching up exists
        //StartCoroutine(FishRoutine());
    }

    void Update()
    {
        Vector3 center = fishManager.transform.position;

        if (Vector3.Distance(transform.position, center) > fishManager.spawnRange)
        {
            if (!turning)
            {
                speed = Random.Range(catchUpSpeed * 0.75f, catchUpSpeed * 1.25f);
            }
            turning = true;
        }
        else
        {
            if (turning)
            {
                speed = Random.Range(avgSpeed * 0.75f, avgSpeed * 1.25f);
            }
            turning = false;
        }

        if (turning)
        {
            Vector3 direction = center - transform.position;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                rotationSpeed * Time.deltaTime);
        }
        else
        {
            // dont always apply, 1 out of 5 frames avg
            if (Random.Range(0, 5) < 1)
            {
                ApplyRules();
            }
        }

        transform.Translate(0, 0, Time.deltaTime * speed);
    }

    //IEnumerator FishRoutine()
    //{
    //    while (true)
    //    {
    //        Vector3 center = fishManager.transform.position;

    //        if (Vector3.Distance(transform.position, center) > fishManager.spawnRange)
    //        {
    //            turning = true;
    //        }
    //        else
    //        {
    //            if (turning)
    //            {
    //                speed = Random.Range(avgSpeed * 0.75f, avgSpeed * 1.25f);
    //            }
    //            turning = false;
    //        }

    //        if (turning)
    //        {
    //            Vector3 direction = center - transform.position;
    //            transform.rotation = Quaternion.Slerp(
    //                transform.rotation,
    //                Quaternion.LookRotation(direction),
    //                rotationSpeed * Time.deltaTime);

    //            speed = Random.Range(catchUpSpeed * 0.75f, catchUpSpeed * 1.25f);
    //        }
    //        else
    //        {
    //            ApplyRules();
    //        }

    //        yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
    //    }
    //}

    void ApplyRules()
    {
        // Rule 1: Go towards group center
        // Rule 2: Move towards goal of the group
        // Rule 3: Avoid other group members

        Vector3 vectorCenter = Vector3.zero;
        Vector3 vectorAvoid = Vector3.zero;
        float groupSpeed = 0;

        Vector3 goalPos = fishManager.GetGoalPosition();

        float dist;

        int groupSize = 0;

        foreach (GameObject obj in fishManager.allFish)
        {
            if (obj != this.gameObject)
            {
                dist = Vector3.Distance(obj.transform.position, this.transform.position);

                if (dist <= neighbourDistance)
                {
                    vectorCenter += obj.transform.position;
                    groupSize++;

                    if (dist < avoidanceDistance)
                    {
                        vectorAvoid += (this.transform.position - obj.transform.position);
                    }

                    FishLogic otherFish = obj.GetComponent<FishLogic>();
                    groupSpeed += otherFish.speed;
                }
            }
        }

        if (groupSize > 0)
        {
            vectorCenter /= groupSize;
            vectorCenter += goalPos - this.transform.position;

            speed = groupSpeed / groupSize;

            Vector3 direction = (vectorCenter + vectorAvoid) - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    rotationSpeed * Time.deltaTime);
            }
        }
    }
}
