using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBlocker : MonoBehaviour
{
    static InputBlocker instance;

    private void Awake() 
    {
        instance = this;

        Hide_Static();
    }

    public static void Show_Static()
    {
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
    }

    public static void Hide_Static()
    {
        instance.gameObject.SetActive(false);
    }
}
