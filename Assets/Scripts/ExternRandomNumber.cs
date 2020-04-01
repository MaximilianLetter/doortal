using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;

public class ExternRandomNumber : MonoBehaviour
{
    //[DllImport("RandomNumberVS")]
    //private static extern int GenerateNumber(int max);

    //[DllImport("native")]
    //private static extern float add(float x, float y);

    [DllImport("native-lib")]
    private static extern int GenerateNumber();

    private Text textComp;

    void Start()
    {
        textComp = GetComponent<Text>();
        StartCoroutine(GenNumber());
        //StartCoroutine(Add());
    }

    //IEnumerator Add()
    //{
    //    while (true)
    //    {
    //        Debug.Log("__________CALL EXTERN FUNCTION_____________");
    //        float x = Random.Range(0.0f, 10.0f);
    //        float y = Random.Range(0.0f, 10.0f);

    //        textComp.text = add(x, y).ToString();

    //        yield return new WaitForSeconds(3);
    //    }
    //}

    IEnumerator GenNumber()
    {
        while (true)
        {
            Debug.Log("__________CALL EXTERN FUNCTION_____________");
            int number = GenerateNumber();

            textComp.text = number.ToString();

            yield return new WaitForSeconds(3);
        }
    }
}
