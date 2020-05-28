using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForOnboardingComplete : MonoBehaviour
{
    public Transform hiddenUntilReady;

    IEnumerator Start()
    {
        OnboardingManager mng = FindObjectOfType<OnboardingManager>();
        
        foreach (Transform child in hiddenUntilReady)
        {
            child.gameObject.SetActive(false);
        }

        while (!mng.GetComplete())
        {
            yield return null;
        }

        foreach (Transform child in hiddenUntilReady)
        {
            child.gameObject.SetActive(true);
        }

        Debug.Log("Buttons -> Ready");
    }
}
